
# 🧠 Name Recall Challenge & Social Memory Simulator

This Unity project simulates memory training tasks for facial-name association and recall, designed to help evaluate short-term memory and cognitive processing related to social interactions.

## 🎮 Overview

Players are shown a sequence of faces, each paired with a name (with optional audio playback). After an encoding phase, they complete a brief distractor task before being asked to recall the names associated with each face.

The recall phase supports two modes:
- **Multiple Choice** (with randomized distractor names)
- **Typed Input** (user types in the remembered name)

Each recall response is measured for:
- **Accuracy**
- **Response Time**
- **System-generated Confidence Rating**

---

## 🧪 Features

- ✅ Randomized faces and name assignments
- ✅ Male/female name generation based on image naming
- ✅ Audio support using pre-recorded name clips
- ✅ Multiple choice or typed input modes
- ✅ Simple math distractor task (with more to come)
- ✅ Confidence-based feedback system
- ✅ Session summary / feedback phase
- ✅ Clean UI with answer buttons
- ✅ Click-to-play audio icons
- ✅ Modular and expandable design for additional cognitive tests

---

## 🗂 Project Structure

- `Scripts/` – Contains core game logic (`GameController.cs`, etc.)
- `Prefabs/` – Includes UI components for face cards, buttons, etc.
- `Resources/Sprites/Faces/` – Realistic face images used during sessions
- `Resources/Audio/Names/` – Optional audio clips for name playback
- `Scenes/` – Main game scene

---

## 🔧 How to Use

1. **Clone the repo** (private — only invited collaborators can access):

2. **Open in Unity** (recommended: Unity 2021.3+)

3. **Play** the scene in the Unity Editor or build as a standalone app.

---

## 🧠 Future Plans

- EEG integration (LSL or serial interface) for real-time brain marker recording
- Session data export (CSV or JSON)
- User profiles and long-term progress tracking
- Additional distractor tasks and memory modalities (sound, words, etc.)

---

## 👤 Author & Collaborators

- Developed by **Ryan R**
https://github.com/Uncrustable125


---

## 📄 License

This project is private and currently under development. Contact the author for permission to use, distribute, or contribute.
