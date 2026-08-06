import React from 'react';
import { AppShell } from '@/features/dashboard/components/AppShell';

export default function AppLayout({ children }: { children: React.ReactNode }) {
  return <AppShell>{children}</AppShell>;
}
