import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth-store';
import { apiClient } from '@/lib/api-client';

export function useAuthGuard() {
  const router = useRouter();
  const { isAuthenticated, setUser } = useAuthStore();
  const [isLoading, setIsLoading] = useState(!isAuthenticated);

  useEffect(() => {
    if (isAuthenticated) {
      setIsLoading(false);
      return;
    }

    let mounted = true;
    
    apiClient<any[]>('/Trees?IncludePrivate=false')
      .then((data) => {
        if (mounted) {
          // Mocking the user profile since there is no explicit /Users/me endpoint
          setUser({
            id: 'current-user',
            email: 'user@familytree.com',
            globalPrivateFlag: false,
            profileData: {},
          });
          setIsLoading(false);
        }
      })
      .catch(() => {
        if (mounted) {
          router.push('/login');
        }
      });
      
    return () => {
      mounted = false;
    };
  }, [isAuthenticated, setUser, router]);

  return { isLoading };
}
