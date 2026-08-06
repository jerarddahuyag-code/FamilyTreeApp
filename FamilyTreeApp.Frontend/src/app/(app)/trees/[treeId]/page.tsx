'use client';

import React, { useState, use } from 'react';
import { ReactFlowProvider, useOnSelectionChange } from '@xyflow/react';
import { TreeCanvas } from '@/features/tree-workspace/components/Canvas/TreeCanvas';
import { WorkspaceSidebar } from '@/features/tree-workspace/components/Sidebar/WorkspaceSidebar';
import { TreeNodeDto } from '@/features/tree-workspace/api/types';
import styles from './trees.module.css';

interface TreeWorkspaceProps {
  params: Promise<{ treeId: string }>;
}

function WorkspaceContent({ treeId }: { treeId: string }) {
  const [selectedNode, setSelectedNode] = useState<TreeNodeDto | null>(null);

  useOnSelectionChange({
    onChange: ({ nodes }) => {
      const selected = nodes.filter(n => n.selected);
      if (selected.length === 1) {
        setSelectedNode(selected[0].data as unknown as TreeNodeDto);
      } else {
        setSelectedNode(null);
      }
    },
  });

  return (
    <div className={styles.workspaceContainer}>
      <div className={styles.canvasArea}>
        <TreeCanvas treeId={treeId} />
      </div>
      <WorkspaceSidebar treeId={treeId} selectedNode={selectedNode} />
    </div>
  );
}

export default function TreeWorkspacePage({ params }: TreeWorkspaceProps) {
  const { treeId } = use(params);

  return (
    <div className={styles.pageContainer}>
      <ReactFlowProvider>
        <WorkspaceContent treeId={treeId} />
      </ReactFlowProvider>
    </div>
  );
}
