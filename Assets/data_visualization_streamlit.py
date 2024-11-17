import streamlit as st
import pandas as pd
import plotly.express as px
from datetime import datetime, timedelta

file_path = r"C:\Users\ayerh\Downloads\temp_generated_energy_data.csv"

st.title("Streamlit: Real-time Data Visualization")
st.write("Time series graph of Electricity Tariff ($/kWh):")

placeholder = st.empty()

try:
    data = pd.read_csv(file_path)

    if "Timestamp" in data.columns and "Electricity Tariff ($/kWh)" in data.columns:
        data["Timestamp"] = pd.to_datetime(data["Timestamp"], errors="coerce")

        one_week_ago = datetime.now() - timedelta(days=7)

        data = data[data["Timestamp"] >= one_week_ago]

        data.dropna(subset=["Timestamp", "Electricity Tariff ($/kWh)"], inplace=True)

        fig = px.line(
            data,
            x="Timestamp",
            y="Electricity Tariff ($/kWh)",
            title="Time Series of Electricity Tariff ($/kWh)",
            labels={"Timestamp": "Time", "Electricity Tariff ($/kWh)": "Electricity Tariff ($/kWh)"}
        )

        placeholder.plotly_chart(fig, use_container_width=True)

        if st.button("Export Visualization as PNG"):
            output_path = r"C:\Users\ayerh\Downloads\water_consumption_visualization.png"
            fig.write_image(output_path)
            st.success(f"Visualization exported as {output_path}")
    else:
        st.error("The dataset does not have the required columns.")
except FileNotFoundError:
    st.warning("Waiting for data file...")