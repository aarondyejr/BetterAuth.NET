// app/page.tsx
"use client";

import { authClient } from "./lib/auth-client";
import { useState } from "react";

export default function Home() {
  const [result, setResult] = useState<string>("");
  const session = authClient.useSession();

  const signUp = async () => {
    const { data, error } = await authClient.signUp.email({
      email: "test@example.com",
      password: "password123",
      name: "Test User",
    });
    setResult(JSON.stringify(error || data, null, 2));
  };

  const signIn = async () => {
    const { data, error } = await authClient.signIn.email({
      email: "dyeaaronjr@proton.me",
      password: "123",
    });
    setResult(JSON.stringify(error || data, null, 2));
  };
  
  const signOut = async () => {
      await authClient.signOut()
  }

  if (session.isPending) return null;

  return (
      <div style={{ padding: 40, fontFamily: "monospace" }}>
        <h1>BetterAuth.NET Test</h1>

        <div style={{ marginBottom: 20 }}>
          <p>Session: {session.data ? session.data.user.email : "Not logged in"}</p>
        </div>

        <button onClick={signUp} style={{ marginRight: 10 }}>Sign Up</button>
        <button style={{ marginRight: 10 }} onClick={signIn}>Sign In</button>
          <button onClick={signOut}>Sign Out</button>

          <pre style={{ marginTop: 20, background: "#111", padding: 20 }}>
    {result ? result : (session.data ? JSON.stringify(session.data.user, null, 2) : "Not logged in")}
</pre>
      </div>
  );
}