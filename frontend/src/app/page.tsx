import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import Link from "next/link";

export default function Home() {
  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold mb-8">🎧 Support Agent Dashboard</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>📞 Calls</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-gray-600 mb-4">View and manage incoming support calls</p>
            <Link href="/calls">
              <Button>View Calls</Button>
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>📊 Analytics</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-gray-600 mb-4">Performance metrics and insights</p>
            <Button variant="outline" disabled>Coming Soon</Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>⚙️ Settings</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-gray-600 mb-4">Configure your support preferences</p>
            <Button variant="outline" disabled>Coming Soon</Button>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
