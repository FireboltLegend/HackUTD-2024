# **EcoVerse: Immersive VR for Sustainable Property Management**

EcoVerse is a cutting-edge platform that leverages immersive VR technology, real-time data, and haptic feedback to improve the sustainability and efficiency of property management. The project allows property managers to remotely monitor energy and water consumption, interact with an AI assistant, and collaborate with technicians—all within a virtual office environment. By combining virtual reality (VR), artificial intelligence (AI), real-time data, and haptic feedback, EcoVerse empowers managers to make data-driven decisions that reduce costs, lower carbon footprints, and enhance operational efficiency.

---

## **Table of Contents**

- [Inspiration](#inspiration)
- [What It Does](#what-it-does)
- [How We Built It](#how-we-built-it)
- [Challenges](#challenges)
- [Accomplishments](#accomplishments)
- [What We Learned](#what-we-learned)
- [What’s Next](#whats-next)
- [Getting Started](#getting-started)
- [Technologies Used](#technologies-used)
- [License](#license)

---

## **Inspiration**

EcoVerse was born from the need to reduce the environmental impact of corporate buildings. As companies increasingly shift towards sustainability, improving energy and water efficiency in office spaces is a pressing challenge. We were motivated by the potential to cut costs, improve operational efficiency, and significantly lower carbon footprints. Through conversations with industry experts and in-depth research, we recognized that combining advanced technology with sustainability could offer a unique solution to this challenge. EcoVerse aims to help businesses reduce their environmental impact while empowering them to make smarter, more informed decisions.

---

## **What It Does**

EcoVerse is an immersive VR platform designed to help property managers optimize the sustainability of their buildings. In this virtual space, property managers can:

- **Explore office spaces in VR:** Walk through detailed virtual replicas of their buildings.
- **Monitor real-time data:** View key metrics on energy and water consumption.
- **Track trends:** Visualize both historical static graphs and dynamic real-time data.
- **Generate AI-driven recommendations:** Get actionable insights based on current building performance to optimize sustainability.
- **Collaborate with technicians:** Invite remote technicians to join the virtual environment for real-time troubleshooting and support.
- **Experience haptic feedback:** Using **bHaptics thermo-tactile gloves**, managers can physically "feel" energy inefficiencies and system adjustments, enhancing their understanding of how different actions impact building performance.

By combining VR, AI, real-time data, and haptics, EcoVerse empowers property managers to make proactive decisions that reduce waste, increase energy efficiency, and create a more sustainable future.

---

## **How We Built It**

The EcoVerse platform was built using a range of technologies to create an immersive, functional experience:

- **Unity**: The core platform for building the interactive VR experience.
- **SambaNova API & Llama**: Power the AI assistant for intelligent, context-aware interactions.
- **Faker API**: Used to generate realistic data sets for energy and water consumption.
- **Streamlit**: For data analysis, real-time visualization, and integration with Unity.
- **Blender & ReadyPlayerMe**: Used to create virtual environments and AI avatars.
- **bHaptics SDK**: Integrated thermo-tactile gloves to provide haptic feedback, allowing users to feel energy inefficiencies and system adjustments.

The combination of these technologies creates a seamless and engaging experience that brings sustainability management to life.

---

## **Challenges**

While developing EcoVerse, we encountered several technical hurdles:

- **Photon Integration**: Implementing **Photon** for multiplayer interactions was challenging, primarily due to network synchronization issues. Ensuring smooth, real-time collaboration between property managers and technicians, without lag or inconsistencies, proved to be complex.
- **Avatar LipSync**: Synchronizing the avatar's mouth movements with AI-generated speech was a key component for realism, and it took time to integrate **LipSync** effectively.
- **Data Stream to Unity**: Sending data from **Streamlit** to **Unity** via **sockets** was particularly difficult, as it was our first time using sockets. The complexity of ensuring smooth, real-time data flow and handling socket implementation posed significant challenges.
- **Hardware Integration**: The integration of **bHaptics** thermo-tactile gloves into the VR environment was tricky, especially when it came to ensuring that the haptic feedback was meaningful and seamless.

---

## **Accomplishments**

We are proud of the following accomplishments:

- Created an **immersive, collaborative VR platform** where property managers and technicians can work together in real-time.
- Developed a **smart AI assistant** that provides real-time data and generates actionable recommendations.
- **Integrated haptic feedback** for the first time using **bHaptics thermo-tactile gloves**, allowing property managers to physically feel the effects of building adjustments.
- Successfully brought together **VR**, **AI**, **real-time data analytics**, and **haptics** into a unified solution.

---

## **What We Learned**

Through this project, we gained valuable insights into:

- Integrating real-time data into a VR environment and making that data actionable.
- Developing AI-driven interactions to provide context-aware recommendations.
- The technical challenges of working with **Photon** for multiplayer integration and **bHaptics** for haptic feedback.
- The importance of collaboration—within our team and with industry experts—to develop a solution that is both innovative and practical.

---

## **What’s Next**

Looking ahead, we have exciting plans for expanding EcoVerse:

- **Remote technician integration**: Allowing technicians to join virtual spaces in real-time for more effective collaboration and troubleshooting.
- **IoT integration**: Enabling automated systems that respond dynamically to real-time data, creating a smarter, more efficient building management system.
- **AR integration**: Exploring augmented reality (AR) to provide even more intuitive ways for property managers to interact with building systems and visualize performance data.
