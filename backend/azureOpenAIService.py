import asyncio
import json
import os
from dotenv import load_dotenv
from rtclient import (
    RTLowLevelClient,
    SessionUpdateMessage,
    ServerVAD,
    SessionUpdateParams,
    ItemCreateMessage,
    FunctionCallOutputItem,
    ResponseCreateMessage,
    InputAudioBufferAppendMessage,
    InputAudioTranscription,
)
from azure.core.credentials import AzureKeyCredential

from tools import RTToolCall, Tool, ToolResultDirection, get_tools

# Load environment variables from .env file
load_dotenv()

active_websocket = None

answer_prompt_system_template = open("agent-instructions.txt", "r").read()
AZURE_OPENAI_SERVICE_ENDPOINT = os.getenv("AZURE_OPENAI_REALTIME_ENDPOINT")
AZURE_OPENAI_SERVICE_KEY = os.getenv("AZURE_OPENAI_API_KEY")
AZURE_OPENAI_DEPLOYMENT_MODEL_NAME = os.getenv(
    "AZURE_OPENAI_REALTIME_DEPLOYMENT_MODEL_NAME"
)

tools: dict[str, Tool] = get_tools()
_tools_pending = {}


async def start_conversation():
    global client
    client = RTLowLevelClient(
        url=AZURE_OPENAI_SERVICE_ENDPOINT,
        key_credential=AzureKeyCredential(AZURE_OPENAI_SERVICE_KEY),
        azure_deployment=AZURE_OPENAI_DEPLOYMENT_MODEL_NAME,
    )
    await client.connect()
    await client.send(
        SessionUpdateMessage(
            session=SessionUpdateParams(
                instructions=answer_prompt_system_template,
                turn_detection=ServerVAD(type="server_vad"),
                voice="coral",
                input_audio_format="pcm16",
                output_audio_format="pcm16",
                # input_audio_transcription=InputAudioTranscription(model="whisper-1"),
                tool_choice="auto",
                tools=[tool.schema for tool in tools.values()],
            )
        )
    )

    asyncio.create_task(receive_messages(client))


async def send_audio_to_external_ai(audioData: str):
    await client.send(
        message=InputAudioBufferAppendMessage(
            type="input_audio_buffer.append", audio=audioData, _is_azure=True
        )
    )


async def receive_messages(client: RTLowLevelClient):
    while not client.closed:
        message = await client.recv()
        if message is None:
            continue
        match message.type:
            case "session.created":
                print("Session Created Message")
                print(f"  Session Id: {message.session.id}")
                pass
            case "error":
                print(f"  Error: {message.error}")
                pass
            case "input_audio_buffer.cleared":
                print("Input Audio Buffer Cleared Message")
                pass
            case "input_audio_buffer.speech_started":
                print(
                    f"Voice activity detection started at {message.audio_start_ms} [ms]"
                )
                await stop_audio()
                pass
            case "input_audio_buffer.speech_stopped":
                pass
            case "response.function_call_arguments.delta":
                print(f"Function Call Arguments Delta: {message.delta}")
                pass
            case "response.function_call_arguments.done":
                print(f"Function Call Arguments Done: {message.arguments}")
                pass
            case "conversation.item.created":
                if message.item and message.item.type == "function_call":
                    if message.item.call_id not in _tools_pending:
                        _tools_pending[message.item.call_id] = RTToolCall(
                            message.item.call_id, message.previous_item_id
                        )
                elif message.item and message.item.type == "function_call_output":
                    print(f"  Tool Output: {message.item.output}")
            case "response.output_item.done":
                if message.item and message.item.type == "function_call":
                    item = message.item
                    tool_call = _tools_pending[message.item.call_id]
                    tool = tools[item.name]
                    args = item.arguments
                    result = await tool.target(json.loads(args))
                    await client.send(
                        ItemCreateMessage(
                            item=FunctionCallOutputItem(
                                call_id=item.call_id,
                                previous_item_id=tool_call.previous_id,
                                output=(
                                    result.to_text()
                                    if result.destination
                                    == ToolResultDirection.TO_SERVER
                                    else ""
                                ),
                            )
                        )
                    )
                    await client.send(
                        ResponseCreateMessage(
                            instructions="Provide the information received from the tool to the user"
                        )
                    )

            case "conversation.item.input_audio_transcription.completed":
                print(f" User:-- {message.transcript}")
            case "conversation.item.input_audio_transcription.failed":
                print(f"  Error: {message.error}")
            case "response.done":
                print("Response Done Message")
                print(f"  Response Id: {message.response.id}")
                if message.response.status_details:
                    print(
                        f"  Status Details: {message.response.status_details.model_dump_json()}"
                    )
            case "response.audio_transcript.done":
                print(f" AI:-- {message.transcript}")
            case "response.audio.delta":
                await receive_audio_for_outbound(message.delta)
                pass
            case _:
                pass


async def init_websocket(socket):
    global active_websocket
    active_websocket = socket


async def receive_audio_for_outbound(data):
    try:
        data = {"Kind": "AudioData", "AudioData": {"Data": data}, "StopAudio": None}

        # Serialize the server streaming data
        serialized_data = json.dumps(data)
        await send_message(serialized_data)

    except Exception as e:
        print(e)


async def stop_audio():
    stop_audio_data = {"Kind": "StopAudio", "AudioData": None, "StopAudio": {}}

    json_data = json.dumps(stop_audio_data)
    await send_message(json_data)


async def send_message(message: str):
    global active_websocket
    try:
        await active_websocket.send(message)
    except Exception as e:
        print(f"Failed to send message: {e}")
