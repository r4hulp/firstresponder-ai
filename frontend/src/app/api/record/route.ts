import { CallRecordingEntity } from "@/models/record";
import { AzureNamedKeyCredential, TableClient } from "@azure/data-tables";

const getTableClient = () => {

  const account = process.env.AZURE_STORAGE_ACCOUNT_NAME || "";
  const key = process.env.AZURE_STORAGE_ACCOUNT_KEY || "";
  const tableName = "records";

  const credential = new AzureNamedKeyCredential(account, key);
  const tableClient = new TableClient(`https://${account}.table.core.windows.net`, tableName, credential);

  return tableClient;

}
export async function GET(request: Request) {
  // returns all the calls in the table
  const { searchParams } = new URL(request.url);
  const rowKey = searchParams.get('rowKey') || "";

  const tableClient = getTableClient();


  const calls = await tableClient.listEntities();

  const callList: CallRecordingEntity[] = [];

  // fetch all the calls
  for await (const entity of calls) {

    // remove the ```markdown from the callSummary
    const callSummary = entity.CallSummary as string || "";
    const callSummaryWithoutMarkdown = callSummary.replace("```markdown", "").replace("```", "");

    const call: CallRecordingEntity = {
      partitionKey: entity.partitionKey as string || "",
      rowKey: entity.rowKey as string || "",
      participants: entity.participants as string[] || [],
      caller: entity.Caller as string || "",
      callDuration: entity.CallDuration as string || "",
      startTime: entity.StartTime as Date || new Date(),
      sessionEndReason: entity.SessionEndReason as string || "",
      transcriptUrl: entity.TranscriptUrl as string || "",
      audioUrl: entity.AudioUrl as string || "",
      eTag: entity.ETag as string || "",
      timestamp: entity.timestamp ? new Date(entity.timestamp as string) : new Date(),
      transcription: entity.Transcription as string || "",
      isTranscribed: entity.IsTranscribed as boolean || false,
      transcribedOn: entity.TranscribedOn as Date || new Date(),
      callSummary: callSummaryWithoutMarkdown,
    }
    callList.push(call);
  }

  // filter by rowKey
  const filteredCalls = callList.filter(call => call.rowKey === rowKey);

  // return first or not found
  if (filteredCalls.length === 0) { 
    return new Response('Call not found', { status: 404 });
  }

  return new Response(JSON.stringify(filteredCalls[0]), { status: 200 });

}




