"use client";

import { CallRecordingEntity } from "@/models/record";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { format } from "date-fns";

interface CallsTableProps {
  calls: CallRecordingEntity[];
  onLoadTranscript: (callId: string) => void;
}

export function CallsTable({ calls, onLoadTranscript }: CallsTableProps) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Caller</TableHead>
            <TableHead>Start Time</TableHead>
            <TableHead>Duration</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {calls.map((call) => (
            <TableRow key={call.rowKey}>
              <TableCell>{call.caller}</TableCell>
              <TableCell>
                {format(new Date(call.startTime), "PPpp")}
              </TableCell>
              <TableCell>{parseInt(call.callDuration)/1000} seconds</TableCell>
              <TableCell>
                <Button
                  variant="outline"
                  className="cursor-pointer"
                  onClick={() => onLoadTranscript(call.rowKey)}
                >
                  Load Transcript
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
} 