'use server';

import { createAzure } from '@ai-sdk/azure';
import { generateText } from 'ai';

const azure = createAzure({
  resourceName: process.env.AZURE_OPENAI_RESOURCE_NAME || "", // Azure resource name
  apiKey: process.env.AZURE_OPENAI_MODEL_KEY || "",
});


export async function generateSummary(transcript: string) {
  const { text } = await generateText({
    model: azure(process.env.AZURE_OPENAI_DEPLOYMENT_NAME || ""),
    prompt: `You are an expert customer support agent quality analyst.
    You are given a transcript of a customer support call.
    Your job is to generate a summary of the call.
    The summary should be in the following format: [Summary of the call]
    Caller Information: [Caller Information]
    Call Duration: [Call Duration]
    Call Type: [Call Type]
    Caller Issue: [Caller Issue]
    Support Agent Resolution: [Support Agent Resolution]
    Caller Feedback: [Caller Feedback]
    Return the summary in markdown format..
    Heres the transcript of the call: ${transcript}
    `,
  });

  return text;
}

