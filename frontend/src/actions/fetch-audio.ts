'use server';
import { BlobSASPermissions, BlobServiceClient, StorageSharedKeyCredential } from '@azure/storage-blob';

const createBlobClient = () => {
  const account = process.env.AZURE_STORAGE_ACCOUNT_NAME || "";
  const key = process.env.AZURE_STORAGE_ACCOUNT_KEY || "";
  const credential = new StorageSharedKeyCredential(account, key);
  const blobServiceClient = new BlobServiceClient(`https://${account}.blob.core.windows.net`, credential);

  return blobServiceClient;

}

const containerName = "records";

export async function sasUrlForAudio(fileName: string): Promise<string | null> {
  try {
    const blobServiceClient = createBlobClient();
    const containerClient = blobServiceClient.getContainerClient(containerName);

    if (!fileName) {
      return null;
    }
    //split fileName by /records/ and take the last part
    const blobName = fileName.split("/records/")[1];

    const blobClient = containerClient.getBlobClient(blobName);

    // generate sas url
    const ONE_HOUR = 1000 * 60 * 60;
    const sasUrl = blobClient.generateSasUrl({
      permissions: BlobSASPermissions.from({ read: true, create: false, write: false }),
      expiresOn: new Date(Date.now() + ONE_HOUR), // 1 hour
    })

    return sasUrl; // Return the file path
  } catch (error) {
    console.error("Error generating SAS URL:", error);
    return null;
  }
}

