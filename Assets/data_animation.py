import streamlit as st
import pandas as pd
import plotly.express as px
from datetime import datetime, timedelta
import random
import time

file_path = r'C:\Users\ayerh\Downloads\temp_generated_energy_data.csv'

st.title("Streamlit: Real-time Data Visualization")
st.write("Animated time series graph of Water Consumption (Liters)")

placeholder = st.empty()

data = pd.read_csv(file_path)

if "Timestamp" in data.columns and "Energy Consumption (kWh)" in data.columns:
    data["Timestamp"] = pd.to_datetime(data["Timestamp"], errors="coerce")

    random_day = random.choice(data["Timestamp"].dt.date.unique())
    selected_day_data = data[data["Timestamp"].dt.date == random_day]
    selected_day_data = selected_day_data[
        (selected_day_data["Timestamp"].dt.hour == 9)
    ]

    consecutive_data = selected_day_data.head(116)

    x_axis_min = consecutive_data["Timestamp"].min()
    x_axis_max = consecutive_data["Timestamp"].max()
    y_axis_min = consecutive_data["Energy Consumption (kWh)"].min()
    y_axis_max = consecutive_data["Energy Consumption (kWh)"].max()

    for i in range(1, len(consecutive_data) + 1):

        partial_data = consecutive_data.head(i)

        fig = px.line(
            partial_data,
            x="Timestamp",
            y="Energy Consumption (kWh)",
            title=f"Time Series of Energy Consumption (kWh) - {i} Points",
            labels={"Timestamp": "Time", "Energy Consumption (kWh)": "Energy Consumption (kWh)"}
        )

        fig.update_layout(
            xaxis_range=[x_axis_min, x_axis_max],
            yaxis_range = [y_axis_min, y_axis_max],
            showlegend=False
        )

        placeholder.plotly_chart(fig, use_container_width=True)

        time.sleep(0.05)

else:
    st.error("The dataset does not have the required columns.")