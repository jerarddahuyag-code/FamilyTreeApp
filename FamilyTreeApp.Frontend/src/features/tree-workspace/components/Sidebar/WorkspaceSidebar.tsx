import { useState } from 'react';
import { TreeNodeDto } from '../../api/types';
import { DetailsPanel } from './DetailsPanel';
import { AddMemberModal } from '../Roster/AddMemberModal';
import { Button } from '@/components/ui/Button/Button';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { useRosterMembers } from '../../api/roster-api';
import styles from './WorkspaceSidebar.module.css';

interface WorkspaceSidebarProps {
  treeId: string;
  selectedNode?: TreeNodeDto | null;
}

export function WorkspaceSidebar({ treeId, selectedNode }: WorkspaceSidebarProps) {
  const [activeTab, setActiveTab] = useState<'roster' | 'details'>('details');
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const { data: rosterData, isLoading } = useRosterMembers(treeId);

  return (
    <div className={styles.sidebar}>
      <div className={styles.tabs}>
        <button 
          className={`${styles.tab} ${activeTab === 'details' ? styles.active : ''}`}
          onClick={() => setActiveTab('details')}
        >
          Details
        </button>
        <button 
          className={`${styles.tab} ${activeTab === 'roster' ? styles.active : ''}`}
          onClick={() => setActiveTab('roster')}
        >
          Roster
        </button>
      </div>

      <div className={styles.content}>
        {activeTab === 'details' && (
          selectedNode ? (
            <DetailsPanel treeId={treeId} selectedNode={selectedNode} />
          ) : (
            <div className={styles.emptyState}>Select a node on the canvas to view details.</div>
          )
        )}
        
        {activeTab === 'roster' && (
          <div className={styles.rosterPanel}>
            <p className={styles.rosterDesc}>Manage the members of this family tree.</p>
            <Button onClick={() => setIsAddModalOpen(true)}>Add New Member</Button>
            
            <div className={styles.rosterList}>
              {isLoading ? (
                <div className={styles.emptyState}>Loading roster...</div>
              ) : rosterData?.items?.length ? (
                rosterData.items.map(member => (
                  <div key={member.familyMemberId} className={styles.rosterListItem}>
                    <Avatar 
                      src={member.profileInfo.avatarUrl || undefined} 
                      name={`${member.profileInfo.firstName} ${member.profileInfo.lastName}`} 
                      size="sm" 
                    />
                    <div className={styles.rosterListInfo}>
                      <span className={styles.rosterListName}>
                        {member.profileInfo.firstName} {member.profileInfo.lastName}
                      </span>
                    </div>
                  </div>
                ))
              ) : (
                <div className={styles.emptyState}>No members found.</div>
              )}
            </div>
          </div>
        )}
      </div>

      <AddMemberModal 
        treeId={treeId} 
        isOpen={isAddModalOpen} 
        onClose={() => setIsAddModalOpen(false)} 
      />
    </div>
  );
}
