// src/app/register/page.tsx
'use client';

import { useState } from 'react';
import { Form, Input, Button, Typography, Alert } from 'antd';
import { useRouter } from 'next/navigation';
import axios from 'axios';
import Link from 'next/link';

const { Title } = Typography;

const RegisterPage = () => {
    const [form] = Form.useForm();
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    const handleSubmit = async (values: any) => {
        setLoading(true);
        setError(null);
        setSuccess(null);
        try {
            await axios.post('https://localhost:7081/api/auth/register', {
                email: values.email,
                firstName: values.firstName,
                lastName: values.lastName,
                password: values.password,
            });
            setSuccess('Registration successful. Please check your email to confirm.');
            setTimeout(() => router.push('/login'), 3000);
        } catch (err: any) {
            setError(err.response?.data?.Message || 'Registration failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ maxWidth: 400, margin: 'auto', padding: '40px 20px' }}>
            <Title level={2}>Register</Title>
            {error && <Alert message={error} type="error" style={{ marginBottom: 16 }} />}
            {success && <Alert message={success} type="success" style={{ marginBottom: 16 }} />}
            <Form form={form} layout="vertical" onFinish={handleSubmit}>
                <Form.Item
                    name="email"
                    label="Email"
                    rules={[
                        { required: true, message: 'Please enter your email' },
                        { type: 'email', message: 'Please enter a valid email' },
                    ]}
                >
                    <Input placeholder="Email" />
                </Form.Item>
                <Form.Item
                    name="firstName"
                    label="First Name"
                    rules={[{ required: true, message: 'Please enter your first name' }]}
                >
                    <Input placeholder="First Name" />
                </Form.Item>
                <Form.Item
                    name="lastName"
                    label="Last Name"
                    rules={[{ required: true, message: 'Please enter your last name' }]}
                >
                    <Input placeholder="Last Name" />
                </Form.Item>
                <Form.Item
                    name="password"
                    label="Password"
                    rules={[
                        { required: true, message: 'Please enter your password' },
                        { min: 6, message: 'Password must be at least 6 characters' },
                    ]}
                >
                    <Input.Password placeholder="Password" />
                </Form.Item>
                <Form.Item
                    name="confirmPassword"
                    label="Confirm Password"
                    dependencies={['password']}
                    rules={[
                        { required: true, message: 'Please confirm your password' },
                        ({ getFieldValue }) => ({
                            validator(_, value) {
                                if (!value || getFieldValue('password') === value) {
                                    return Promise.resolve();
                                }
                                return Promise.reject(new Error('Passwords do not match'));
                            },
                        }),
                    ]}
                >
                    <Input.Password placeholder="Confirm Password" />
                </Form.Item>
                <Form.Item>
                    <Button type="primary" htmlType="submit" loading={loading} block>
                        Register
                    </Button>
                </Form.Item>
                <Form.Item>
                    <Link href="/login">Already have an account? Login</Link>
                </Form.Item>
            </Form>
        </div>
    );
};

export default RegisterPage;
