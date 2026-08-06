import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useAddMember } from '../../api/roster-api';
import { useAddTreeNode } from '../../api/canvas-api';
import { Modal } from '@/components/ui/Modal/Modal';
import { Button } from '@/components/ui/Button/Button';
import { Gender, VisibilityStatus } from '../../api/types';
import styles from './AddMemberModal.module.css';

interface AddMemberModalProps {
  treeId: string;
  isOpen: boolean;
  onClose: () => void;
}

interface FormValues {
  firstName: string;
  lastName: string;
  gender: Gender | '';
  visibilityStatus: VisibilityStatus;
}

export function AddMemberModal({ treeId, isOpen, onClose }: AddMemberModalProps) {
  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<FormValues>({
    defaultValues: {
      firstName: '',
      lastName: '',
      gender: '',
      visibilityStatus: 'Visible'
    }
  });

  const { mutateAsync: addMember } = useAddMember();
  const { mutateAsync: addTreeNode } = useAddTreeNode();
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (data: FormValues) => {
    try {
      setError(null);
      // 1. Biological (Roster) layer API call
      const { familyMemberId } = await addMember({
        treeId,
        claimedByUserId: null,
        profileInfo: {
          firstName: data.firstName,
          lastName: data.lastName,
          gender: data.gender as Gender || null,
          birthDate: null,
          avatarUrl: null,
          phoneNumber: null,
          bio: null
        },
        visibilityStatus: data.visibilityStatus
      });

      // 2. Visual (Canvas) layer API call
      await addTreeNode({
        treeId,
        nodeType: 'Single',
        x: 0,
        y: 0,
        familyMemberIds: [familyMemberId]
      });

      reset();
      onClose();
    } catch (e: any) {
      setError(e.message || 'An error occurred while adding the member.');
    }
  };

  if (!isOpen) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose}>
      <div className={styles.modalContent}>
        <h2>Add Family Member</h2>
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
          {error && <div className={styles.error}>{error}</div>}
          
          <div className={styles.formGroup}>
            <label>First Name</label>
            <input className={styles.input} {...register('firstName', { required: 'First name is required' })} />
            {errors.firstName && <span className={styles.errorText}>{errors.firstName.message}</span>}
          </div>
          
          <div className={styles.formGroup}>
            <label>Last Name</label>
            <input className={styles.input} {...register('lastName')} />
          </div>

          <div className={styles.formGroup}>
            <label>Gender</label>
            <select className={styles.select} {...register('gender')}>
              <option value="">Select gender</option>
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="NonBinary">Non-binary</option>
              <option value="PreferNotToSay">Prefer not to say</option>
            </select>
          </div>

          <div className={styles.formGroup}>
            <label>Visibility</label>
            <select className={styles.select} {...register('visibilityStatus')}>
              <option value="Visible">Visible</option>
              <option value="Hidden">Hidden</option>
              <option value="Pending">Pending (requires claim)</option>
            </select>
          </div>

          <div className={styles.actions}>
            <Button onClick={onClose} type="button">Cancel</Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Adding...' : 'Add Member'}
            </Button>
          </div>
        </form>
      </div>
    </Modal>
  );
}
