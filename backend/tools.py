import json
from enum import Enum
from logging import info
from typing import Any, Callable, Literal

import aiohttp


class ToolResultDirection(Enum):
    TO_SERVER = 1
    TO_CLIENT = 2


class ToolResult:
    text: str
    destination: ToolResultDirection

    def __init__(self, text: str, destination: ToolResultDirection):
        self.text = text
        self.destination = destination

    def to_text(self) -> str:
        if self.text is None:
            return ""
        return self.text if isinstance(self.text, str) else json.dumps(self.text)


class RTToolCall:
    tool_call_id: str
    previous_id: str

    def __init__(self, tool_call_id: str, previous_id: str):
        self.tool_call_id = tool_call_id
        self.previous_id = previous_id


class Tool:
    target: Callable[..., ToolResult]
    schema: Any

    def __init__(self, target: Any, schema: Any):
        self.target = target
        self.schema = schema


_save_customer_issue_tool_schema = {
    "name": "save_customer_issue",
    "type": "function",
    "description": "Saves a customer issue to the database",
    "parameters": {
        "type": "object",
        "properties": {
            "customer_name": {
                "type": "string",
                "description": "The name of the customer",
            },
            "customer_company": {
                "type": "string",
                "description": "The company of the customer",
            },
            "issue_description": {
                "type": "string",
                "description": "The description of the issue",
            },
            "issue_type": {
                "type": "string",
                "description": "The type of issue, new or existing",
            },
        },
        "required": ["customer_name", "issue_description", "issue_type"],
        "additionalProperties": False,
    },
}


async def _save_customer_issue_tool(args: Any) -> ToolResult:
    info(f'Saving customer issue for "{args["customer_name"]}".')
    return ToolResult(
        json.dumps("The ticket has been updated."), ToolResultDirection.TO_SERVER
    )


def get_tools() -> dict[str, Tool]:

    return {
        # "get_current_weather": Tool(
        #     schema=_weather_current_tool_schema,
        #     target=lambda args: _weather_tool("current", args),
        # ),
        # "get_weather_forecast": Tool(
        #     schema=_weather_forecast_tool_schema,
        #     target=lambda args: _weather_tool("hourly", args),
        # ),
        "save_customer_issue": Tool(
            schema=_save_customer_issue_tool_schema,
            target=lambda args: _save_customer_issue_tool(args),
        ),
    }
