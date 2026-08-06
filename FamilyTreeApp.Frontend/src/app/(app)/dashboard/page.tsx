'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { apiClient } from '@/lib/api-client';
import { useToast } from '@/components/ui/Toast/ToastProvider';
import { Spinner } from '@/components/ui/Spinner/Spinner';

interface TreeInfo {
  treeId: string;
  name: string;
  description: string;
}

interface TreeList {
  trees: TreeInfo[];
}

export default function DashboardPage() {
  const [trees, setTrees] = useState<TreeInfo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const toast = useToast();

  useEffect(() => {
    // Fetch available trees for the current user
    apiClient<{ value: TreeList, isSuccess: boolean }>('/trees')
      .then((res) => {
        if (res && res.isSuccess) {
          setTrees(res.value.trees);
        } else {
          toast.error('Failed to load trees');
        }
      })
      .catch(() => {
        toast.error('Error fetching trees');
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, [toast]);

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: '2rem' }}>
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Dashboard</h1>
      <p>Select a tree to open the workspace:</p>

      {trees.length === 0 ? (
        <p>You don't have access to any trees yet.</p>
      ) : (
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginTop: '1rem' }}>
          {trees.map(tree => (
            <Link key={tree.treeId} href={`/trees/${tree.treeId}`} style={{ textDecoration: 'none' }}>
              <div style={{
                border: '1px solid var(--border-color)',
                borderRadius: '8px',
                padding: '1.5rem',
                width: '300px',
                cursor: 'pointer',
                backgroundColor: 'var(--bg-secondary)',
                color: 'var(--text-primary)'
              }}>
                <h3 style={{ margin: '0 0 0.5rem 0' }}>{tree.name}</h3>
                <p style={{ margin: 0, color: 'var(--text-secondary)' }}>{tree.description}</p>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
