import { BaseEdge, EdgeProps, getSmoothStepPath, Edge } from '@xyflow/react';
import { TreeEdgeDto } from '../../api/types';

export type FamilyEdgeData = Edge<TreeEdgeDto, 'familyEdge'>;

export function FamilyEdge({
  sourceX,
  sourceY,
  targetX,
  targetY,
  sourcePosition,
  targetPosition,
  style = {},
  markerEnd,
}: EdgeProps<FamilyEdgeData>) {
  const [edgePath] = getSmoothStepPath({
    sourceX,
    sourceY,
    sourcePosition,
    targetX,
    targetY,
    targetPosition,
  });

  return (
    <BaseEdge 
      path={edgePath} 
      markerEnd={markerEnd} 
      style={{ ...style, stroke: 'var(--color-edge-parent-child)', strokeWidth: 2 }} 
    />
  );
}
