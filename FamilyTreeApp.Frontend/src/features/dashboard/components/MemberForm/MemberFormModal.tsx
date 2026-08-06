import React, { useState } from 'react';
import { Modal } from '@/components/ui/Modal/Modal';
import { Button } from '@/components/ui/Button/Button';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { apiClient } from '@/lib/api-client';
import { useTreeStore } from '@/stores/tree-store';
import { useToast } from '@/components/ui/Toast/ToastProvider';
import styles from './MemberForm.module.css';

const memberSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  birthDate: z.string().optional(),
  deathDate: z.string().optional(),
  gender: z.enum(['Male', 'Female', 'Other']).optional(),
});

type MemberFormData = z.infer<typeof memberSchema>;

interface MemberFormModalProps {
  treeId: string;
  isOpen: boolean;
  onClose: () => void;
}

export const MemberFormModal: React.FC<MemberFormModalProps> = ({ treeId, isOpen, onClose }) => {
  const { addFamilyMember } = useTreeStore();
  const toast = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const { register, handleSubmit, formState: { errors }, reset } = useForm<MemberFormData>({
    resolver: zodResolver(memberSchema)
  });

  const onSubmit = async (data: MemberFormData) => {
    setIsSubmitting(true);
    try {
      const payload = {
        treeId,
        profileInfo: {
          firstName: data.firstName,
          lastName: data.lastName,
          birthDate: data.birthDate || null,
          gender: data.gender || null,
          bio: null,
          avatarUrl: null,
          phoneNumber: null
        },
        visibilityStatus: 'Hidden'
      };

      const result = await apiClient<{ value: string, isSuccess: boolean }>(`/trees/${treeId}/members`, {
        method: 'POST',
        body: JSON.stringify(payload)
      });

      if (result && result.isSuccess) {
        addFamilyMember({
          familyMemberId: result.value,
          treeId,
          claimedByUserId: null,
          profileInfo: payload.profileInfo as any,
          visibilityStatus: 'Hidden',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString()
        });
        toast.success('Member added successfully');
        reset();
        onClose();
      } else {
        toast.error('Failed to add member');
      }
    } catch (error) {
      toast.error('Failed to add member');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Add Family Member">
      <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
        <div className={styles.row}>
          <div className={styles.field}>
            <label>First Name</label>
            <input type="text" {...register('firstName')} className={styles.input} />
            {errors.firstName && <span className={styles.error}>{errors.firstName.message}</span>}
          </div>
          <div className={styles.field}>
            <label>Last Name</label>
            <input type="text" {...register('lastName')} className={styles.input} />
            {errors.lastName && <span className={styles.error}>{errors.lastName.message}</span>}
          </div>
        </div>

        <div className={styles.row}>
          <div className={styles.field}>
            <label>Birth Date</label>
            <input type="date" {...register('birthDate')} className={styles.input} />
          </div>
          <div className={styles.field}>
            <label>Death Date (Optional)</label>
            <input type="date" {...register('deathDate')} className={styles.input} />
          </div>
        </div>

        <div className={styles.field}>
          <label>Gender</label>
          <select {...register('gender')} className={styles.input}>
            <option value="">Select...</option>
            <option value="Male">Male</option>
            <option value="Female">Female</option>
            <option value="Other">Other</option>
          </select>
        </div>

        <div className={styles.actions}>
          <Button type="button" variant="ghost" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            Add Member
          </Button>
        </div>
      </form>
    </Modal>
  );
};
