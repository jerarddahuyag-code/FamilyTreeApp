import { Handle, Position, NodeProps, Node } from '@xyflow/react';
import { TreeNodeDto } from '../../api/types';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import styles from './FamilyMemberNode.module.css';

export type FamilyMemberNodeData = Node<TreeNodeDto, 'familyMember'>;

export function FamilyMemberNode({ data, selected }: NodeProps<FamilyMemberNodeData>) {
  return (
    <div className={`${styles.nodeWrapper} ${selected ? styles.selected : ''}`}>
      <Handle type="target" position={Position.Top} className={styles.handle} />
      <div className={styles.membersContainer}>
        {data.members.map((member) => (
          <div key={member.id} className={styles.member}>
            <Avatar 
              src={member.profileInfo.avatarUrl || undefined} 
              name={member.isMasked ? 'Anonymous' : `${member.profileInfo.firstName} ${member.profileInfo.lastName}`} 
              size="md"
            />
            <div className={styles.memberInfo}>
              <span className={styles.name}>
                {member.isMasked ? 'Anonymous Node' : `${member.profileInfo.firstName || ''} ${member.profileInfo.lastName || ''}`}
              </span>
            </div>
          </div>
        ))}
      </div>
      <Handle type="source" position={Position.Bottom} className={styles.handle} />
    </div>
  );
}
