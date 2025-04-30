'use client';

import { useEffect, useState, useRef } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { CallRecordingEntity } from '@/models/record';
import { Button } from '@/components/ui/button';
import { ArrowLeft, Play, Pause, LucidePhoneCall } from 'lucide-react';
import { Transcript } from '@/models/transcript';
import { sasUrlForAudio } from '@/actions/fetch-audio';
import { generateSummary } from '@/actions/generate-summary';
import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

export default function CallTranscriptPage() {
  const { call_id } = useParams();
  const router = useRouter();
  const [transcript, setTranscript] = useState<Transcript | null>(null);
  const [loading, setLoading] = useState(true);
  const [callTranscript, setCallTranscript] = useState<CallRecordingEntity | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [sasUrl, setSasUrl] = useState<string | null>(null);
  const [currentTime, setCurrentTime] = useState(0);
  const audioRef = useRef<HTMLAudioElement>(null);
  const transcriptContainerRef = useRef<HTMLDivElement>(null);
  const [supportAgentId, setSupportAgentId] = useState<number | null>(1);

  useEffect(() => {
    const fetchTranscript = async () => {
      try {
        const response = await fetch(`/api/record?rowKey=${call_id}`);
        if (!response.ok) {
          throw new Error('Failed to fetch transcript');
        }
        const data: CallRecordingEntity = await response.json();


        if (data.transcription) { 
          setTranscript(JSON.parse(data.transcription) as Transcript);
        }
        setCallTranscript(data);


      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchTranscript();
  }, [call_id]);

  useEffect(() => {
    const fetchAudio = async () => {
      const sasUrl = await sasUrlForAudio(callTranscript?.audioUrl || "");
      setSasUrl(sasUrl);
    };

    fetchAudio();

  }, [callTranscript]);

  useEffect(() => {
    if (transcript) {
      const supportAgent = transcript?.phrases.find(phrase => phrase.text.includes('calling mobile support'));

      console.log(supportAgent);
      if (supportAgent) {
        setSupportAgentId(supportAgent.speaker);
      }
    }
  }, [transcript]);

  const playSegment = (startTime: number, duration: number) => {
    if (audioRef.current) {
      audioRef.current.currentTime = startTime / 1000; // Convert to seconds
      audioRef.current.play();

      // Stop after the segment duration
      setTimeout(() => {
        if (audioRef.current) {
          audioRef.current.pause();
        }
      }, duration);
    }
  };

  // Auto-scroll to current phrase
  useEffect(() => {
    if (transcriptContainerRef.current) {
      const currentPhraseElement = transcriptContainerRef.current.querySelector('.ring-2');
      if (currentPhraseElement) {
        currentPhraseElement.scrollIntoView({
          behavior: 'smooth',
          block: 'center'
        });
      }
    }
  }, [currentTime]);

  if (loading) {
    return (
      <div className="container mx-auto p-4">
        <Card>
          <CardHeader>
            <Skeleton className="h-8 w-[200px]" />
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Summary Skeleton */}
              <div className="space-y-4">
                <Skeleton className="h-6 w-[150px]" />
                <div className="space-y-3">
                  <Skeleton className="h-4 w-full" />
                  <Skeleton className="h-4 w-[90%]" />
                  <Skeleton className="h-4 w-[80%]" />
                  <Skeleton className="h-4 w-[95%]" />
                </div>
              </div>

              {/* Transcript Skeleton */}
              <div className="space-y-4">
                <Skeleton className="h-6 w-[150px]" />
                <Skeleton className="h-4 w-[120px] mb-6" />
                <div className="space-y-4">
                  {[...Array(5)].map((_, i) => (
                    <div key={i} className="flex w-full">
                      <div className="max-w-[80%] bg-gray-100 rounded-2xl p-4">
                        <Skeleton className="h-4 w-[100px] mb-2" />
                        <Skeleton className="h-4 w-[80%]" />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-4">
        <Card>
          <CardHeader>
            <CardTitle>Error</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-red-500">{error}</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      <Button
        variant="ghost"
        className="mb-6"
        onClick={() => router.back()}
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Back to Calls
      </Button>

      {sasUrl && (
        <Card className="mb-4">
          <CardHeader>
            <CardTitle><LucidePhoneCall className="inline-block h-4 w-4" /> Call Recording </CardTitle>
          </CardHeader>
          <CardContent>
            <audio
              controls
              className="w-full"
              src={sasUrl}
              ref={audioRef}
              onTimeUpdate={(e) => {
                setCurrentTime(e.currentTarget.currentTime * 1000); // Convert to milliseconds
              }}
            >
              Your browser does not support the audio element.
            </audio>
          </CardContent>
        </Card>
      )}
      <Card className="max-h-[70vh] overflow-y-auto">
        <CardHeader>
          <CardTitle>Call Details</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

            {/* Transcript Section */}
            <div className="space-y-4  overflow-y-auto">
              <CardTitle>Call Transcript</CardTitle>
              {transcript ? (
                <>
                  <div className="text-sm text-gray-500 mb-6">
                    <p>Total Duration: {transcript.durationMilliseconds / 1000} seconds</p>
                  </div>
                  <div
                    ref={transcriptContainerRef}
                    className="space-y-4  max-h-[50vh] overflow-y-auto pr-2 scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-gray-100"
                  >
                    {transcript.phrases.map((phrase, index) => {
                      const isSupportAgent = phrase.speaker === supportAgentId;
                      const isCurrentPhrase = currentTime >= phrase.offsetMilliseconds &&
                        currentTime < (phrase.offsetMilliseconds + phrase.durationMilliseconds);

                      return (
                        <div
                          key={index}
                          className={`flex w-full ${isSupportAgent ? 'justify-start' : 'justify-end'} items-center mb-2`}
                        >
                          <div
                            className={`max-w-[55%] ${isSupportAgent ? 'bg-blue-100 rounded-2xl' : 'bg-gray-100 rounded-2xl'} p-2 relative group transition-all duration-200 ${isCurrentPhrase ? 'ring-1 ring-blue-500' : ''}`}
                          >
                            <div className="text-xs font-medium mb-0.5 text-gray-600">
                              {isSupportAgent ? 'Support Agent' : 'Caller'}
                            </div>
                            <p className="text-gray-800 leading-relaxed text-sm">
                              {phrase.words.map((word, wordIndex) => {
                                const isCurrentWord = currentTime >= word.offsetMilliseconds && 
                                  currentTime < (word.offsetMilliseconds + word.durationMilliseconds);
                                
                                return (
                                  <span 
                                    key={wordIndex} 
                                    className={`${isCurrentWord ? 'bg-yellow-200 font-medium' : ''} inline-block px-0.5`}
                                  >
                                    {word.text}
                                  </span>
                                );
                              })}
                            </p>
                            <div className="flex items-center justify-end gap-1 mt-1">
                              <Button
                                variant="ghost"
                                size="icon"
                                className={`h-5 w-5 hover:bg-blue-200/50 active:bg-blue-200 ${isCurrentPhrase ? 'bg-blue-100' : ''}`}
                                onClick={() => playSegment(phrase.offsetMilliseconds, phrase.durationMilliseconds)}
                              >
                                {isCurrentPhrase ? (
                                  <Pause className="h-3 w-3 text-blue-600 animate-pulse" />
                                ) : (
                                  <Play className="h-3 w-3 text-blue-600" />
                                )}
                              </Button>
                              <span className="text-[10px] text-gray-400">
                                {Math.floor(phrase.offsetMilliseconds / 60000)}:{(phrase.offsetMilliseconds % 60000).toString().padStart(2, '0')}
                              </span>
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </>
              ) : (
                <p>No transcript available</p>
              )}
            </div>
            {/* Summary Section */}
            {callTranscript?.callSummary && (
              <div className="space-y-4 ">
                <CardTitle>Call Summary</CardTitle>
                <Markdown remarkPlugins={[remarkGfm]}>{callTranscript.callSummary}</Markdown>
              </div>
            )}

          </div>
        </CardContent>
      </Card>
    </div>
  );
} 