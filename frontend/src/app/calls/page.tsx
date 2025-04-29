"use client";

import { CallsTable } from "@/components/calls-table";
import { CallRecordingEntity } from "@/models/record";
import { Loader2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";



export default function CallsPage() {
  const [calls, setCalls] = useState<CallRecordingEntity[]>([]);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    setLoading(true);
    fetch('/api/records')
      .then(res => res.json())
      .then(data => {
        setCalls(data.sort((a: CallRecordingEntity, b: CallRecordingEntity) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime()));
        setLoading(false);
      });
  }, []);

  const handleLoadTranscript = async (callId: string) => {
    // TODO: Implement transcript loading logic
    console.log("Loading transcript for call:", callId);
    router.push(`/calls/${callId}`);
  };

  return (
    <div className="container mx-auto py-10">
      <h1 className="text-3xl font-bold mb-6">Calls</h1>
      {loading ? (
        <div className="flex justify-center items-center h-screen">
          <Loader2 className="h-8 w-8 animate-spin" />
        </div>
      ) : (
        <CallsTable calls={calls} onLoadTranscript={handleLoadTranscript} />
      )}
    </div>
  );
}
