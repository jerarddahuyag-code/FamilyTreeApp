'use client';

import React from 'react';
import { useAuthGuard } from '@/features/auth/hooks/useAuthGuard';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { useAuthStore } from '@/stores/auth-store';
import styles from './AppShell.module.css';
import Link from 'next/link';
import { LogOut } from 'lucide-react';

export function AppShell({ children }: { children: React.ReactNode }) {
  const { isLoading } = useAuthGuard();
  const { user } = useAuthStore();

  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <div className={styles.spinner}></div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.logo}>
          <Link href="/dashboard">FamilyTree</Link>
        </div>
        <div className={styles.userSection}>
          <Avatar name={user?.email || 'User'} size="sm" />
          <a href="/api/Auth/logout" className={styles.logoutBtn}>
            <LogOut size={18} />
          </a>
        </div>
      </header>
      <main className={styles.main}>{children}</main>
    </div>
  );
}
