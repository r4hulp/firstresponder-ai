export interface CallRecordingEntity {
    partitionKey: string; // e.g., Date or CallerID
    rowKey: string; // Unique Call ID
    participants: string[];
    caller: string;
    callDuration: string; // in seconds
    startTime: Date;
    sessionEndReason: string;
    transcriptUrl: string; // URL if stored in Blob Storage
    audioUrl: string;
    eTag: string;
    timestamp?: Date;
    transcription: string;
    isTranscribed: boolean;
    transcribedOn: Date;
    callSummary: string;
}

