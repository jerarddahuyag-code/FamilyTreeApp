import React from 'react';
import styles from './Spinner.module.css';

export interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export const Spinner: React.FC<SpinnerProps> = ({ size = 'md', className = '' }) => {
  const classNames = [
    styles.spinner,
    styles[size],
    className,
  ].filter(Boolean).join(' ');

  return (
    <div className={classNames}>
      <div className={styles.circle}></div>
    </div>
  );
};
