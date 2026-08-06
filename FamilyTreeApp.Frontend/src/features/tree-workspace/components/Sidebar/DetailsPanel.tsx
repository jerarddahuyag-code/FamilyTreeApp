import { useForm } from 'react-hook-form';
import { useState, useEffect } from 'react';
import { TreeNodeDto, CanvasMemberDto, Gender } from '../../api/types';
import { useUpdateMemberProfile } from '../../api/roster-api';
import { Button } from '@/components/ui/Button/Button';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import styles from './WorkspaceSidebar.module.css';

interface DetailsPanelProps {
  treeId: string;
  selectedNode: TreeNodeDto;
}

export function DetailsPanel({ treeId, selectedNode }: DetailsPanelProps) {
  // If multi-person node, we pick the first member to edit for simplicity in Phase 5
  // Later we can add a tab selector per member in the node
  const member = selectedNode.members[0];
  
  if (!member) {
    return <div className={styles.emptyState}>No member data</div>;
  }

  return (
    <div className={styles.detailsPanel}>
      <MemberEditor treeId={treeId} member={member} />
    </div>
  );
}

function MemberEditor({ treeId, member }: { treeId: string; member: CanvasMemberDto }) {
  const { register, handleSubmit, reset, formState: { isSubmitting } } = useForm({
    defaultValues: {
      firstName: member.profileInfo.firstName || '',
      lastName: member.profileInfo.lastName || '',
      gender: member.profileInfo.gender || '',
    }
  });

  const { mutateAsync: updateProfile } = useUpdateMemberProfile();
  const [isEditing, setIsEditing] = useState(false);

  useEffect(() => {
    reset({
      firstName: member.profileInfo.firstName || '',
      lastName: member.profileInfo.lastName || '',
      gender: member.profileInfo.gender || '',
    });
    setIsEditing(false);
  }, [member, reset]);

  const onSubmit = async (data: any) => {
    await updateProfile({
      treeId,
      familyMemberId: member.id,
      profileInfo: {
        ...member.profileInfo,
        firstName: data.firstName,
        lastName: data.lastName,
        gender: data.gender as Gender || null,
      }
    });
    setIsEditing(false);
  };

  if (member.isMasked) {
    return (
      <div className={styles.maskedDetails}>
        <Avatar size="lg" />
        <h3 className={styles.maskedName}>Anonymous Member</h3>
        <p>You do not have permission to view or edit this member.</p>
      </div>
    );
  }

  return (
    <div className={styles.memberEditor}>
      <div className={styles.avatarSection}>
        <Avatar src={member.profileInfo.avatarUrl || undefined} name={`${member.profileInfo.firstName} ${member.profileInfo.lastName}`} size="lg" />
      </div>

      {!isEditing ? (
        <div className={styles.viewMode}>
          <div className={styles.field}>
            <span className={styles.label}>Name</span>
            <span className={styles.value}>{member.profileInfo.firstName} {member.profileInfo.lastName}</span>
          </div>
          <div className={styles.field}>
            <span className={styles.label}>Gender</span>
            <span className={styles.value}>{member.profileInfo.gender || 'Not specified'}</span>
          </div>
          <div className={styles.actions}>
            <Button onClick={() => setIsEditing(true)}>Edit Details</Button>
          </div>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className={styles.editMode}>
          <div className={styles.formGroup}>
            <label>First Name</label>
            <input {...register('firstName')} className={styles.input} />
          </div>
          <div className={styles.formGroup}>
            <label>Last Name</label>
            <input {...register('lastName')} className={styles.input} />
          </div>
          <div className={styles.formGroup}>
            <label>Gender</label>
            <select {...register('gender')} className={styles.select}>
              <option value="">Select gender</option>
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="NonBinary">Non-binary</option>
              <option value="PreferNotToSay">Prefer not to say</option>
            </select>
          </div>
          <div className={styles.actions}>
            <Button variant="secondary" onClick={() => setIsEditing(false)} type="button">Cancel</Button>
            <Button type="submit" disabled={isSubmitting}>Save</Button>
          </div>
        </form>
      )}
    </div>
  );
}
