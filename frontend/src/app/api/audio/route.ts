import { NextRequest, NextResponse } from 'next/server';

export async function GET(request: NextRequest) {
  const { searchParams } = new URL(request.url);
  const callId = searchParams.get('callId');

  if (!callId) {
    return NextResponse.json({ error: 'Call ID is required' }, { status: 400 });
  }

  const audioUrl = `https://your-storage-account.blob.core.windows.net/your-container/your-audio-file.wav`;

  return NextResponse.json({ audioUrl });
}
