'use client';

import React, { useState } from 'react';
import { useTreeStore } from '@/stores/tree-store';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { Button } from '@/components/ui/Button/Button';
import { Plus, Search } from 'lucide-react';
import styles from './Roster.module.css';

interface RosterListProps {
  onAddMember: () => void;
}

export const RosterList: React.FC<RosterListProps> = ({ onAddMember }) => {
  const { roster } = useTreeStore();
  const [search, setSearch] = useState('');

  const memberList = Object.values(roster).filter(m => {
    const name = `${m.profileInfo?.firstName || ''} ${m.profileInfo?.lastName || ''}`.trim();
    return name.toLowerCase().includes(search.toLowerCase());
  });

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Family Members</h2>
        <Button size="sm" onClick={onAddMember} leftIcon={<Plus size={16} />}>
          Add Member
        </Button>
      </div>

      <div className={styles.searchContainer}>
        <Search className={styles.searchIcon} size={16} />
        <input 
          type="text" 
          placeholder="Search members..." 
          className={styles.searchInput}
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
      </div>

      <div className={styles.list}>
        {memberList.length === 0 ? (
          <div className={styles.emptyState}>No members found</div>
        ) : (
          memberList.map(member => (
            <div key={member.familyMemberId} className={styles.listItem}>
              <Avatar name={`${member.profileInfo?.firstName || ''} ${member.profileInfo?.lastName || ''}`} src={member.profileInfo?.avatarUrl || undefined} />
              <div className={styles.itemInfo}>
                <div className={styles.itemName}>{member.profileInfo?.firstName} {member.profileInfo?.lastName}</div>
                <div className={styles.itemDates}>
                  {member.profileInfo?.birthDate ? new Date(member.profileInfo.birthDate).getFullYear() : 'Unknown'} - Present
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
