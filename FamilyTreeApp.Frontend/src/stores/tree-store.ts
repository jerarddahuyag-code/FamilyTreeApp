import { create } from 'zustand';

export type Gender = 'Male' | 'Female' | 'NonBinary' | 'PreferNotToSay';
export type VisibilityStatus = 'Hidden' | 'Pending' | 'Visible';
export type NodeType = 'Single' | 'Partner' | 'MultiPerson';

export interface ProfileInfo {
  firstName: string | null;
  lastName: string | null;
  gender: Gender | null;
  birthDate: string | null;
  bio: string | null;
  avatarUrl: string | null;
  phoneNumber: string | null;
}

export interface FamilyMemberDto {
  familyMemberId: string;
  treeId: string;
  claimedByUserId: string | null;
  profileInfo: ProfileInfo;
  visibilityStatus: VisibilityStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CanvasMemberDto {
  id: string;
  profileInfo: ProfileInfo;
  isMasked: boolean;
  visibilityStatus: VisibilityStatus;
}

export interface TreeNodeDto {
  id: string;
  type: NodeType;
  position: { x: number; y: number };
  members: CanvasMemberDto[];
}

export interface TreeEdgeDto {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
}

interface TreeState {
  nodes: Record<string, TreeNodeDto>;
  edges: Record<string, TreeEdgeDto>;
  roster: Record<string, FamilyMemberDto>;
  isLoading: boolean;
  setCanvasData: (nodes: TreeNodeDto[], edges: TreeEdgeDto[]) => void;
  setRosterData: (members: FamilyMemberDto[]) => void;
  addNode: (node: TreeNodeDto) => void;
  updateNodePosition: (id: string, position: { x: number; y: number }) => void;
  addEdge: (edge: TreeEdgeDto) => void;
  removeNode: (id: string) => void;
  removeEdge: (id: string) => void;
  addFamilyMember: (member: FamilyMemberDto) => void;
  updateFamilyMember: (id: string, updates: Partial<FamilyMemberDto>) => void;
}

export const useTreeStore = create<TreeState>((set) => ({
  nodes: {},
  edges: {},
  roster: {},
  isLoading: false,
  
  setCanvasData: (nodes, edges) => {
    const nodesMap: Record<string, TreeNodeDto> = {};
    nodes.forEach(n => { nodesMap[n.id] = n; });
    
    const edgesMap: Record<string, TreeEdgeDto> = {};
    edges.forEach(e => { edgesMap[e.id] = e; });
    
    set({ nodes: nodesMap, edges: edgesMap, isLoading: false });
  },

  setRosterData: (members) => {
    const rosterMap: Record<string, FamilyMemberDto> = {};
    members.forEach(m => { rosterMap[m.familyMemberId] = m; });
    set({ roster: rosterMap });
  },

  addNode: (node) => set((state) => ({
    nodes: { ...state.nodes, [node.id]: node }
  })),

  updateNodePosition: (id, position) => set((state) => ({
    nodes: {
      ...state.nodes,
      [id]: { ...state.nodes[id], position }
    }
  })),

  addEdge: (edge) => set((state) => ({
    edges: { ...state.edges, [edge.id]: edge }
  })),

  removeNode: (id) => set((state) => {
    const newNodes = { ...state.nodes };
    delete newNodes[id];
    // Remove edges connected to this node
    const newEdges = Object.fromEntries(
      Object.entries(state.edges).filter(([_, edge]) => 
        edge.sourceNodeId !== id && edge.targetNodeId !== id
      )
    );
    return { nodes: newNodes, edges: newEdges };
  }),

  removeEdge: (id) => set((state) => {
    const newEdges = { ...state.edges };
    delete newEdges[id];
    return { edges: newEdges };
  }),

  addFamilyMember: (member) => set((state) => ({
    roster: { ...state.roster, [member.familyMemberId]: member }
  })),

  updateFamilyMember: (id, updates) => set((state) => ({
    roster: {
      ...state.roster,
      [id]: { ...state.roster[id], ...updates }
    }
  }))
}));
