export type NodeType = 'Single' | 'Partner' | 'MultiPerson';
export type VisibilityStatus = 'Hidden' | 'Pending' | 'Visible';
export type Gender = 'Male' | 'Female' | 'NonBinary' | 'PreferNotToSay';
export type TreeRole = 'Owner' | 'Admin' | 'Member';
export type RelationshipType = 'Parent' | 'Child' | 'Sibling' | 'Spouse';

export type ApiResponse<T> = {
  value: T;
  isSuccess: boolean;
  isFailure: boolean;
  error: {
    code: string;
    message: string;
  } | null;
};

export type ProfileInfo = {
  firstName: string | null;
  lastName: string | null;
  birthDate: string | null;
  avatarUrl: string | null;
  phoneNumber: string | null;
  gender: Gender | null;
  bio: string | null;
}

export interface CanvasCoordinates {
  x: number;
  y: number;
}

export type CanvasMemberDto = {
  id: string;
  profileInfo: ProfileInfo;
  isMasked: boolean;
  visibilityStatus: VisibilityStatus;
};

export type TreeNodeDto = {
  id: string;
  type: NodeType;
  position: CanvasCoordinates;
  members: CanvasMemberDto[];
};

export type TreeEdgeDto = {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
};

export interface GetCanvasQueryResponse {
  nodes: TreeNodeDto[];
  edges: TreeEdgeDto[];
}

export interface NodePositionUpdate {
  nodeId: string;
  x: number;
  y: number;
}

export interface UpdateCanvasCommand {
  treeId: string;
  updates: NodePositionUpdate[];
}

export interface AddTreeNodeCommand {
  treeId: string;
  nodeType: NodeType;
  x: number;
  y: number;
  familyMemberIds: string[] | null;
}

export interface AddTreeEdgeCommand {
  treeId: string;
  sourceNodeId: string;
  targetNodeId: string;
}

export interface GetFamilyMembersResponseItem {
  familyMemberId: string;
  treeId: string;
  claimedByUserId: string | null;
  profileInfo: ProfileInfo;
  visibilityStatus: VisibilityStatus;
  createdAt: string;
  updatedAt: string;
}

export interface GetFamilyMembersResponse {
  items: GetFamilyMembersResponseItem[];
}

export interface AddFamilyMemberCommand {
  treeId: string;
  claimedByUserId: string | null;
  profileInfo: ProfileInfo;
  visibilityStatus: VisibilityStatus;
}

export interface UpdateFamilyMemberProfileCommand {
  treeId: string;
  familyMemberId: string;
  profileInfo: ProfileInfo;
}

export interface UpdateFamilyMemberClaimedUserCommand {
  treeId: string;
  familyMemberId: string;
  claimedByUserId: string | null;
}
