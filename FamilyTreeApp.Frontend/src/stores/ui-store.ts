import { create } from 'zustand';

interface UIState {
  sidebarOpen: boolean;
  activeTab: 'roster' | 'settings' | 'details';
  selectedNodeId: string | null;
  toggleSidebar: () => void;
  setActiveTab: (tab: 'roster' | 'settings' | 'details') => void;
  setSelectedNode: (id: string | null) => void;
}

export const useUIStore = create<UIState>((set) => ({
  sidebarOpen: true,
  activeTab: 'roster',
  selectedNodeId: null,
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  setActiveTab: (tab) => set({ activeTab: tab }),
  setSelectedNode: (id) => set({ selectedNodeId: id }),
}));
