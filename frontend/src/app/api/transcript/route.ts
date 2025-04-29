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
  // get the callId from the request
  const { searchParams } = new URL(request.url);
  const rowKey = searchParams.get('rowKey') || "";
  const partitionKey = searchParams.get('partitionKey') || "";

  const tableClient = getTableClient();

  if (!rowKey || !partitionKey) {
    return new Response('rowKey and partitionKey are required', { status: 400 });
  }

  const callRecordingEntity = await tableClient.getEntity(partitionKey, rowKey);

  if (!callRecordingEntity) {
    return new Response('Call recording entity not found', { status: 404 });
  }

  const transcription = callRecordingEntity.transcription;

  return new Response(JSON.stringify(transcription), { status: 200 });

}