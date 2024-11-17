import speech_recognition as sr
import openai
from openai import OpenAI
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

client = OpenAI(
	api_key="add13954-7af9-4c07-b0d3-5e3f3ed4fd6f",
	base_url="https://api.sambanova.ai/v1",
)

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
			with open(os.path.join(os.path.dirname(__file__), "user.txt"), "w", encoding='utf-8') as file:
				file.write(recognized_text)
			return recognized_text
			
		except sr.WaitTimeoutError: 
			print("WaitTimeOutError")
			return ""

def samby(messages, model='Meta-Llama-3.1-8B-Instruct', temperature=0.1, top_p = 0.1):
	response = client.chat.completions.create(
		model=model,
		messages=messages,
		temperature=temperature,
		top_p = top_p
	)
	response_text = response.choices[0].message.content


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
	saveResponse("Say Something!")
	print("Say Something!")
	userResponse = getaudio()  # Capture user speech
	
	if "recommendation" in userResponse.lower():
		recommendation_message = (
			"I noticed that your electricity tariff last week was high quite at about $0.45 per kWh, "
			"which is well above the expected average of $0.17 to $0.35. Therefore, please consider "
			"upgrading the lighting to energy-efficient LED lamps and optimize HVAC systems with smart "
			"thermostats and occupancy sensors."
		)
		saveResponse(recommendation_message)
		tts(recommendation_message)
	else:
		response = samby(conversation)  # Get response from the model
		saveResponse(response)
		tts(response)
	
	# Wait until the sync file is set to "b" before continuing
	while True:
		if readSyncFile() == "b":
			break
		time.sleep(0.5)