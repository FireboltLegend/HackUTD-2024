import speech_recognition as sr
import openai
import os
import pygame
import random
import sounddevice as sd
import numpy as np
import wave
from google.cloud import texttospeech
import time

conversation = []

gtts_api_key = "AIzaSyDB70UuSAy0Gd6fxu3tF5mLp9NufPyulKw"

openai.api_key = "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"
openai.api_base = 'https://api.openai.com/v1'

def create_tts_client(api_key: str):
	client_options = {"api_key": api_key}
	return texttospeech.TextToSpeechClient(client_options=client_options)

tts_client = create_tts_client(gtts_api_key)

def text_to_wav(voice_name: str, text: str):
	language_code = "-".join(voice_name.split("-")[:2])
	text_input = texttospeech.SynthesisInput(text=text)
	voice_params = texttospeech.VoiceSelectionParams(
		language_code=language_code, name=voice_name
	)
	audio_config = texttospeech.AudioConfig(audio_encoding=texttospeech.AudioEncoding.LINEAR16)

	response = tts_client.synthesize_speech(
		input=text_input,
		voice=voice_params,
		audio_config=audio_config,
	)
	
	filename = os.path.join(os.path.dirname(__file__), "Resources\\audio.wav")
	with open(filename, "wb") as out:
		out.write(response.audio_content)
		# print(f'Generated speech saved to "{filename}"')

def openFile(filepath):
	with open(filepath, 'r', encoding='utf-8') as infile:
		return infile.read()

def getaudio():
	recognizer = sr.Recognizer()
	microphone = sr.Microphone()

	with microphone as source:
		recognizer.adjust_for_ambient_noise(source)
		try:
			audio = recognizer.listen(source, timeout=3)
			recognized_text = recognizer.recognize_google(audio)
			conversation.append({'role': 'user', 'content': recognized_text})
			print("User:", recognized_text)
			return recognized_text
			
		except sr.WaitTimeoutError: 
			print("WaitTimeOutError")
			return ""

def gpt3(messages, model='gpt-3.5-turbo', temperature=0.9, max_tokens=100, frequency_penalty=2.0, presence_penalty=2.0):
	response = openai.ChatCompletion.create(
		model=model,
		messages=messages,
		temperature=temperature,
		max_tokens=max_tokens,
		frequency_penalty=frequency_penalty,
		presence_penalty=presence_penalty
	)
	response_text = response['choices'][0]['message']['content'].strip()

	return response_text

def playAudio():
	pygame.mixer.init()
	pygame.mixer.music.load("Resources\\audio.wav")
	pygame.mixer.music.play()
	while pygame.mixer.music.get_busy():
		pygame.time.Clock().tick(10)
	pygame.mixer.quit()

def tts(response):
	print("Tofu:", response)
	text_to_wav("en-US-Standard-H", response)
	conversation.append({'role': 'system', 'content': response})
	writeSyncFile("a")
	# playAudio()

def saveResponse(response):
	with open(os.path.join(os.path.dirname(__file__), "speaker.txt"), "w", encoding='utf-8') as file:
		file.write(response)

def readSyncFile():
	with open(os.path.join(os.path.dirname(__file__), "sync.txt"), "r", encoding='utf-8') as file:
		return file.read()

def writeSyncFile(text):
	with open(os.path.join(os.path.dirname(__file__), "sync.txt"), "w", encoding='utf-8') as file:
		file.write(text)

conversation.append({'role': 'system', 'content': openFile(os.path.join(os.path.dirname(__file__), "Chat.txt"))})

tWelcome = "My name is Tofu!"
saveResponse(tWelcome)
tts(tWelcome)

while True:
	if readSyncFile() == "b":
		break
	time.sleep(0.5)

greetings = [
	"How’s your day going?",
	"What’s been happening with you?",
	"How’s everything with you today?",
	"Got anything interesting going on?",
	"How’s life treating you?",
	"What's new with you?",
	"How’ve things been?",
	"How’s your day been?"
]


selectedGreeting = random.randint(0, len(greetings) - 1)
saveResponse(greetings[selectedGreeting])
tts(greetings[selectedGreeting])

while True:
	if readSyncFile() == "b":
		break
	time.sleep(0.5)

while True:
	saveResponse("Say Something!")
	print("Say Something!")
	userResponse = getaudio()		
	response = gpt3(conversation)
	saveResponse(response)
	tts(response)
	
	while True:
		if readSyncFile() == "b":
			break
		time.sleep(0.5)

	count = 0