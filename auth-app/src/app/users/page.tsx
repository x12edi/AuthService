// src/app/users/page.tsx
'use client';

import { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Select, Typography, Input } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import axios from 'axios';
import withAuth from '@/components/withAuth';
import DashboardLayout from '@/components/DashboardLayout';

const { Title } = Typography;
const { Option } = Select;

interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
}

const UsersPage = () => {
    const [users, setUsers] = useState<User[]>([]);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(false);
    const [isAdmin, setIsAdmin] = useState(false);
    const [editModalVisible, setEditModalVisible] = useState(false);
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const [form] = Form.useForm();
    const [pagination, setPagination] = useState({ current: 1, pageSize: 10 });
    const [sorter, setSorter] = useState({ field: 'email', order: 'asc' });
    const [filters, setFilters] = useState({ email: '', role: '' });

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            axios
                .get('https://localhost:7081/api/user/profile', {
                    headers: { Authorization: `Bearer ${token}` },
                })
                .then((res) => {
                    console.log("grid", res);
                    setIsAdmin(res.data.role === 'Admin');
                })
                .catch((err) => {
                    console.error('Failed to fetch profile:', err);
                });
        }
        fetchUsers();
    }, [pagination, sorter, filters]);

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const response = await axios.get('https://localhost:7081/api/user/list', {
                params: {
                    sortBy: sorter.field,
                    sortOrder: sorter.order === 'descend' ? 'desc' : 'asc',
                    emailFilter: filters.email,
                    roleFilter: filters.role,
                    page: pagination.current,
                    pageSize: pagination.pageSize,
                },
                headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
            });
            console.log(response);
            setUsers(response.data.users);
            setTotal(response.data.TotalCount);
        } catch (err) {
            console.error('Failed to fetch users:', err);
            //setUsers([
            //    { Id: '1', Email: 'test1@example.com', FirstName: 'Test', LastName: 'One', Role: 'Admin' },
            //    { Id: '2', Email: 'test2@example.com', FirstName: 'Test', LastName: 'Two', Role: 'User' },
            //]);
            //setTotal(2);
        } finally {
            setLoading(false);
        }
    };

    const handleTableChange = (pagination: any, filters: any, sorter: any) => {
        setPagination({ current: pagination.current, pageSize: pagination.pageSize });
        setSorter({ field: sorter.field, order: sorter.order });
        setFilters({ email: filters.email ? filters.email[0] : '', role: filters.role ? filters.role[0] : '' });
    };

    const handleEdit = (user: User) => {
        setSelectedUser(user);
        form.setFieldsValue({ role: user.role });
        setEditModalVisible(true);
    };

    const handleSave = async () => {
        try {
            const values = await form.validateFields();
            await axios.put(
                'https://localhost:7081/api/user/role',
                { userId: selectedUser?.id, role: values.Role },
                { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
            );
            setEditModalVisible(false);
            fetchUsers();
        } catch (err) {
            console.error('Failed to update role:', err);
        }
    };

    const columns = [
        {
            title: 'ID',
            dataIndex: 'id',
            key: 'id',
        },
        {
            title: 'Email',
            dataIndex: 'email',
            key: 'email',
            sorter: true,
            filterDropdown: ({ setSelectedKeys, selectedKeys, confirm, clearFilters }: any) => (
                <div style={{ padding: 8 }}>
                    <Input
                        placeholder="Search Email"
                        value={selectedKeys[0]}
                        onChange={(e) => setSelectedKeys(e.target.value ? [e.target.value] : [])}
                        onPressEnter={() => confirm()}
                        style={{ width: 188, marginBottom: 8, display: 'block' }}
                    />
                    <Button type="primary" onClick={() => confirm()} size="small" style={{ width: 90, marginRight: 8 }}>
                        Search
                    </Button>
                    <Button onClick={() => { clearFilters(); setSelectedKeys([]); confirm(); }} size="small" style={{ width: 90 }}>
                        Reset
                    </Button>
                </div>
            ),
            onFilter: (value: any, record: User) => true,
        },
        {
            title: 'First Name',
            dataIndex: 'firstName',
            key: 'firstName',
        },
        {
            title: 'Last Name',
            dataIndex: 'lastName',
            key: 'lastName',
        },
        {
            title: 'Role',
            dataIndex: 'role',
            key: 'role',
            sorter: true,
            filters: [
                { text: 'Admin', value: 'Admin' },
                { text: 'User', value: 'User' },
            ],
            onFilter: (value: any, record: User) => true,
        },
        {
            title: 'Actions',
            key: 'actions',
            render: (_: any, record: User) =>
                isAdmin ? (
                    <Button icon={<EditOutlined />} onClick={() => handleEdit(record)} />
                ) : null,
        },
    ];

    return (
        <DashboardLayout>
            <Title level={2}>Users</Title>
            <Table
                columns={columns}
                dataSource={users}
                rowKey="id"
                pagination={{ ...pagination, total }}
                loading={loading}
                onChange={handleTableChange}
            />
            <Modal
                title="Edit User Role"
                open={editModalVisible}
                onOk={handleSave}
                onCancel={() => setEditModalVisible(false)}
            >
                <Form form={form} layout="vertical">
                    <Form.Item
                        name="Role"
                        label="Role"
                        rules={[{ required: true, message: 'Please select a role!' }]}
                    >
                        <Select>
                            <Option value="Admin">Admin</Option>
                            <Option value="User">User</Option>
                        </Select>
                    </Form.Item>
                </Form>
            </Modal>
        </DashboardLayout>
    );
};

export default withAuth(UsersPage);
