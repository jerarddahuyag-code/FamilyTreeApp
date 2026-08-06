import React from 'react';
import styles from './Badge.module.css';

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: 'owner' | 'admin' | 'member' | 'public' | 'private';
}

export const Badge: React.FC<BadgeProps> = ({ variant = 'member', className = '', children, ...props }) => {
  const classNames = [
    styles.badge,
    styles[variant],
    className,
  ].filter(Boolean).join(' ');

  return (
    <span className={classNames} {...props}>
      {children}
    </span>
  );
};
