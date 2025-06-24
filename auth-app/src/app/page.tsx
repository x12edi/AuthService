// src/app/page.tsx
'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

const HomePage = () => {
    const router = useRouter();

    useEffect(() => {
        //const token = localStorage.getItem('token');
        //router.replace(token ? '/dashboard' : '/login');
        router.replace('/dashboard');
    }, [router]);

    return null;
};

export default HomePage;