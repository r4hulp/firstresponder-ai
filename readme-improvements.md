# 🔊 AI Voice Agent Platform – Azure-Powered Realtime Call Intelligence

This project enables intelligent, real-time, two-way communication between human callers and an AI voice agent using **Azure Communication Services (ACS)** and **OpenAI's Realtime API**. It is a production-grade reference architecture for building modern AI-powered support systems, with post-call analytics and actionable insights.

---

## 📚 Table of Contents

- [🔊 AI Voice Agent Platform – Azure-Powered Realtime Call Intelligence](#-ai-voice-agent-platform--azure-powered-realtime-call-intelligence)
  - [📚 Table of Contents](#-table-of-contents)
  - [🌐 Overview](#-overview)
  - [🧠 Key Components](#-key-components)
    - [1. Azure Communication Services (ACS)](#1-azure-communication-services-acs)
    - [2. OpenAI Realtime API Integration](#2-openai-realtime-api-integration)
    - [3. Call Orchestrator (Azure Functions)](#3-call-orchestrator-azure-functions)
    - [4. Azure AI Speech Pipeline](#4-azure-ai-speech-pipeline)
    - [5. Storage Services](#5-storage-services)
    - [6. Monitoring Dashboard](#6-monitoring-dashboard)
  - [⚙️ Workflow](#️-workflow)
  - [🧹 Extensibility](#-extensibility)
  - [🚀 Benefits](#-benefits)
  - [📁 Diagram](#-diagram)
  - [📌 Requirements](#-requirements)
  - [📞 Use Cases](#-use-cases)
  - [🚰 Future Enhancements](#-future-enhancements)
  - [🧑‍💼 Contributors](#-contributors)

---

## 🌐 Overview

The application allows a user to call a hotline number connected to Azure Communication Services (ACS). The call is routed via **websockets** to **OpenAI Realtime API**, enabling intelligent two-way interaction through a voice agent. Meanwhile, multiple Azure components work in tandem to capture, transcribe, analyze, and present the interaction data in a centralized **dashboard** for operations, quality monitoring, and product improvements.

---

## 🧠 Key Components

### 1. Azure Communication Services (ACS)
- Acts as the backbone for real-time voice communication.
- Handles telephony integration and emits lifecycle events like:
  - `incomingCall`
  - `callConnected`
  - `callDisconnected`
  - `callRecordingReady`

### 2. OpenAI Realtime API Integration
- Real-time two-way conversation over WebSocket.
- Powers the AI voice agent logic (intent handling, fallback, context retention).
- Connects with third-party APIs if required for task execution.

### 3. Call Orchestrator (Azure Functions)
- Event-driven logic that responds to ACS events.
- Responsible for:
  - Initiating the call flow
  - Persisting call metadata to Azure Table Storage
  - Storing call recordings in Azure Blob Storage
  - Enqueuing calls for post-processing

### 4. Azure AI Speech Pipeline
A custom speech analytics pipeline that performs:
- **Transcription** of recorded calls.
- **Speaker diarization** (who said what, when).
- **Summary and Action Plan Generation** using NLP models.

### 5. Storage Services
- **Blob Storage**: Stores raw call recordings (audio).
- **Table Storage**: Stores call metadata (caller ID, timestamps, call duration, status).

### 6. Monitoring Dashboard
- Frontend to view and interact with call logs.
- Features:
  - Call playback
  - Transcription view with speaker separation
  - Summary and Action Plan
  - Search and filter by customer, outcome, duration
  - Quality monitoring tools

---

## ⚙️ Workflow

1. **Caller initiates a call** to the hotline number connected to ACS.
2. **ACS** connects the call and emits `incomingCall` event.
3. **Call Orchestrator** (Azure Function) listens to events and begins orchestration:
   - Connects to OpenAI Realtime API for voice-agent interaction
   - Stores call details to Table storage
   - On `callRecordingReady`, stores the audio in Blob storage and enqueues for processing.
4. **Azure AI Speech Pipeline**:
   - Transcribes the call
   - Performs speaker diarization
   - Generates a structured summary and plan of action
5. **Dashboard** displays:
   - Full call log, transcription, summary
   - Allows human review, callbacks, and annotation.

---

## 🧹 Extensibility

This architecture is designed to be modular and extensible:
- 🔍 **Sentiment Analysis** using Azure Text Analytics
- 🌍 **Multilingual Support** using Azure Translator
- 🧠 **Knowledge Integration** with custom vector databases (e.g., Azure AI Search)
- 🗂 **Call Reports & Exports** in PDF or CSV
- 🛠 **Fallback to Human Agent** trigger
- 🔗 **CRM Integrations** (Dynamics, Salesforce, etc.)

---

## 🚀 Benefits

- **Full AI Agent Lifecycle**: Live conversation, recording, analysis, and action plan generation.
- **Scalable**: Uses serverless components (Azure Functions, Blob/Table storage).
- **Secure**: Built on Azure platform with enterprise-grade security.
- **Observability**: Rich dashboard for quality monitoring and intervention.

---

## 📁 Diagram

![Architecture Diagram](./d3410882-5d00-45e7-906a-5904226b08fb.png)

---

## 📌 Requirements

- Azure Subscription
- Azure Communication Services resource
- OpenAI Realtime API key and endpoint
- Azure Functions App (with event trigger)
- Storage Accounts (Blob + Table)
- Optional: Azure Cognitive Services for Speech, Text Analytics

---

## 📞 Use Cases

- IT Support Hotline with auto-triaging
- Healthcare appointment bot
- Customer service agent quality auditing
- Sales call qualification and logging
- Smart voice assistants in logistics

---

## 🚰 Future Enhancements

- Live sentiment tracking during calls
- Real-time alerts for agent escalation
- Integration with Microsoft Teams
- Call clustering and analytics dashboards using Power BI

---

## 🧑‍💼 Contributors

Built by the AI & Cloud Architecture team at [Your Organization Name Here]

