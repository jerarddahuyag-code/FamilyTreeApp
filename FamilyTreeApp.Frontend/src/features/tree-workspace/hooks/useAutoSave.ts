import { useEffect, useState, useCallback } from 'react';
import { useUpdateCanvas } from '../api/canvas-api';
import { useReactFlow } from '@xyflow/react';
import { NodePositionUpdate } from '../api/types';

export function useAutoSave(treeId: string) {
  const { mutate: updateCanvas, isPending } = useUpdateCanvas();
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const { getNodes } = useReactFlow();

  const markUnsaved = useCallback(() => {
    setHasUnsavedChanges(true);
  }, []);

  const saveLayout = useCallback(() => {
    if (!hasUnsavedChanges) return;

    const nodes = getNodes();
    const updates: NodePositionUpdate[] = nodes.map(node => ({
      nodeId: node.id,
      x: node.position.x,
      y: node.position.y
    }));

    updateCanvas({ treeId, updates }, {
      onSuccess: () => {
        setHasUnsavedChanges(false);
      }
    });
  }, [hasUnsavedChanges, getNodes, treeId, updateCanvas]);

  // Auto-save every 5 minutes if there are changes
  useEffect(() => {
    const intervalId = setInterval(() => {
      setHasUnsavedChanges((currentHasUnsaved) => {
        if (currentHasUnsaved) {
          saveLayout();
        }
        return currentHasUnsaved;
      });
    }, 5 * 60 * 1000);

    return () => clearInterval(intervalId);
  }, [saveLayout]);

  return {
    saveLayout,
    hasUnsavedChanges,
    markUnsaved,
    isSaving: isPending
  };
}
