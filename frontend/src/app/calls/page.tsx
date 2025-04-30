"use client";

import { CallsTable } from "@/components/calls-table";
import { CallRecordingEntity } from "@/models/record";
import { Loader2, ArrowLeft, Phone } from "lucide-react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useEffect, useState } from "react";
import { Card, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

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
      <div className="mb-8">
        <Button variant="ghost" asChild className="mb-4">
          <Link href="/" className="flex items-center gap-2">
            <ArrowLeft className="h-4 w-4" />
            <span>Back</span>
          </Link>
        </Button>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <div className="flex items-center gap-2">
              <Phone className="h-6 w-6 text-primary" />
              <CardTitle className="text-2xl font-bold">Call Recordings</CardTitle>
            </div>
          </CardHeader>
          <CardDescription className="px-6 pb-4">
            View and manage your call recordings. Click on a call to view its transcript.
          </CardDescription>
        </Card>
      </div>

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
