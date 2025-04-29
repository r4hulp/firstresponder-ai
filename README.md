# FirstResponder-AI

FirstResponder-AI is a real-time, low-latency voice agent powered by OpenAI's Realtime APIs and Azure Communication Services. Leveraging advanced Speech-to-Speech (S2S) architectures and multimodal AI models, it enables natural, human-like conversations with minimal delay. This solution is designed to revolutionize customer support by providing instant, scalable, and highly personalized voice interactions.

## Overview

FirstResponder-AI delivers 24/7 availability, instant multilingual support, and seamless scalability, offering up to 90% cost savings per interaction compared to traditional systems. By replacing legacy IVR and human agent systems, it dramatically reduces wait times, improves resolution rates, boosts customer satisfaction, and allows human agents to focus on more complex tasks. The system also integrates Azure AI Speech for advanced transcription, speaker diarization, and analytics, enabling features like call summaries, timestamped transcriptions, sentiment analysis, and actionable insights.

## Key Benefits

- **Real-Time, Low-Latency Conversations:** Natural, human-like interactions with minimal delay.
- **24/7 Availability:** Always-on support for customers, regardless of time zone.
- **Cost Efficiency:** Up to 90% savings per interaction compared to human agents.
- **Instant Multilingual Support:** Supports multiple languages out of the box.
- **Seamless Scalability:** Effortlessly handles fluctuating call volumes.
- **Consistent Service Delivery:** Uniform quality and experience for every customer.
- **Superior to Traditional IVR:** Faster, more natural, and more effective than legacy systems.

## Core Capabilities

- **Speech-to-Speech (S2S) AI:** Enables direct, real-time voice conversations using advanced AI models.
- **Multimodal AI:** Integrates voice, text, and other modalities for richer interactions.
- **Transcription & Speaker Diarization:** Uses Azure AI Speech to transcribe calls, identify speakers, and generate timestamped transcripts.
- **Call Summaries & Analytics:** Automatically generates call summaries, sentiment analysis, and actionable insights for quality review and follow-up.
- **Personalization:** Adapts responses based on customer context and history.
- **Integration Ready:** Easily connects with third-party systems and dashboards for analysis, quality, and review.

## Agent Instructions Structure

The `backend/agent-instructions.txt` file defines the core behavior, personality, and conversation flow for the AI agent. This file is structured into clear sections, including personality and tone, supported languages, task definitions, conversation pacing, and detailed state-based conversation flows. Each state outlines the agent's actions, required information, and transitions to the next step, making it easy to understand and modify the agent's logic.

By editing or extending this file, developers can rapidly prototype and deploy robust, domain-specific agents. You can adjust the agent's demeanor, supported languages, verification steps, or even add new conversation states to handle unique business requirements. This modular approach enables the creation of highly adaptable and reliable AI agents tailored to a wide range of customer support scenarios.

## Architecture

```mermaid
flowchart LR
    subgraph Frontend
        Caller([Caller])
        Dashboard(["Dashboard<br/>(Analysis, Quality, Review)"])
    end

    subgraph "Azure Services"
        ACS([Azure Communication Service])
    end

    subgraph Backend
        Orchestrator([Call Orchestrator])
        OpenAI(["Azure OpenAI Realtime API<br/>(websocket)"])
        ThirdParty(["Third Party Integrations"])
        RecorderFn(["recorder-fn<br/>(Transcribe, Speaker Diarization,<br/>Generate Call Summary, Plan of Actions)"])
    end

    subgraph "Storage & Processing"
        Queue([Queue])
        Blob([Blob])
        Table([Table])
    end

    Caller --> ACS
    ACS -- events --> Orchestrator
    ACS -- websocket --> OpenAI
    OpenAI --> ThirdParty

    Orchestrator -- transcriptionQueue --> Queue
    Orchestrator -- callRecordings --> Blob
    Orchestrator -- callInformation --> Table

    Queue -- recordingAvailable --> RecorderFn
    Blob -- recordingAvailable --> RecorderFn

    RecorderFn --> Dashboard

    Blob --> Dashboard
    Table --> Dashboard
```

## Call Recording and Human Feedback Loop

FirstResponder-AI leverages Azure Communication Service's call recording functionality to capture every customer interaction. These recordings are processed through the Azure AI Speech pipeline, enabling advanced features such as transcription, speaker diarization, and sentiment analysis. The resulting data—including detailed transcripts, identified speakers, sentiment scores, and call summaries—is made available via the dashboard. This empowers human agents to review, audit, and intervene in complex cases, ensuring high-quality support and continuous improvement of the AI agent. The seamless integration of automated analysis and human feedback creates a robust, closed-loop system for exceptional customer service.

## Features

- **AI-Powered Support:** Intelligent assistance for hardware and subscription issues
- **Bilingual Support:** Natural conversation in multiple languages
- **Real-time Communication:** Built with Azure Communication Services that extends support to various communication channels
- **Modern UI:** Built with Next.js 15 and React 19 (for rapid prototyping and development)
- **Event Processing:** Integration with Azure Event Grid

## Tech Stack

### Frontend
- Next.js 15
- React 19
- TypeScript
- Tailwind CSS
- Azure SDK for JavaScript
- AI SDK

### Backend
- Python
- Quart (Async web framework)
- Azure OpenAI Service
- Azure Communication Services
- Azure Event Grid
- Azure Storage

## Project Structure

```
firstresponder-ai/
├── frontend/           # Next.js frontend application
├── backend/           # Python backend service
├── recorder-fn/       # Recording functionality
└── .github/          # GitHub workflows and configurations
```

## Getting Started

### Prerequisites

- Node.js (for frontend)
- Python 3.x (for backend)
- Azure subscription with necessary services enabled

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend
   ```

2. Create and activate a virtual environment:
   ```bash
   python -m venv .venv
   source .venv/bin/activate  # On Windows: .venv\Scripts\activate
   ```

3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

4. Start the backend server:
   ```bash
   python main.py
   ```

## Environment Variables

Both frontend and backend require environment variables to be set up. Create `.env` files in both directories with the necessary configuration. See the respective README files in each directory for specific requirements.

### Frontend (`frontend/.env`)

```env
# API endpoint for backend communication
NEXT_PUBLIC_API_URL=http://localhost:8000

# Azure Communication Services connection string (if used in frontend)
NEXT_PUBLIC_ACS_CONNECTION_STRING=your_acs_connection_string

# Any other public keys or config needed by the frontend
```

### Backend (`backend/.env`)

```env
# Azure OpenAI Service
AZURE_OPENAI_API_KEY=your_openai_api_key
AZURE_OPENAI_ENDPOINT=https://your-openai-resource.openai.azure.com/

# Azure Communication Services
AZURE_COMMUNICATION_SERVICE_CONNECTION_STRING=your_acs_connection_string

# Azure Storage
AZURE_STORAGE_CONNECTION_STRING=your_storage_connection_string

# Event Grid
AZURE_EVENT_GRID_TOPIC_ENDPOINT=your_event_grid_topic_endpoint
AZURE_EVENT_GRID_KEY=your_event_grid_key

# Other backend-specific settings
PORT=8000
DEBUG=True
```

### Recorder Function (`recorder-fn/local.settings.json` or `.env`)

If your recorder-fn project uses Azure Functions, configuration is typically in `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "your_storage_connection_string",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ACS_CONNECTION_STRING": "your_acs_connection_string",
    "TRANSCRIBE_API_KEY": "your_transcribe_api_key"
  }
}
```

Or, if you use a `.env` file for local development:

```env
AZURE_STORAGE_CONNECTION_STRING=your_storage_connection_string
ACS_CONNECTION_STRING=your_acs_connection_string
TRANSCRIBE_API_KEY=your_transcribe_api_key
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, please open an issue in the GitHub repository or contact the maintainers.

