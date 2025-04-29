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

  const tableClient = getTableClient();


  const calls = tableClient.listEntities();

  const callList: CallRecordingEntity[] = [];

  // fetch all the calls
  for await (const entity of calls) {
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
    }
    callList.push(call);
  }

  return new Response(JSON.stringify(callList), { status: 200 });

}




