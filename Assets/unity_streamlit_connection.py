import streamlit as st
import socket

def get_data_from_unity():
    host = "127.0.0.1"  # Unity's server IP (localhost)
    port = 5000         # Port matching Unity

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((host, port))
        data = s.recv(1024)  # Receive up to 1024 bytes
        return data.decode('utf-8')

st.title("Streamlit Receiving Data from Unity")
st.write("Press the button to fetch data from Unity.")

if st.button("Fetch Data"):
    unity_data = get_data_from_unity()
    st.write(f"Data from Unity: {unity_data}")