// src/app/dashboard/page.tsx
'use client'
import { Card, Col, Row, Typography } from 'antd';
import withAuth from '@/components/withAuth';
import DashboardLayout from '@/components/DashboardLayout';

const { Title } = Typography;

const DashboardPage = () => {
    return (
        <DashboardLayout>
            <Title level={2}>Dashboard</Title>
            <Row gutter={16}>
                <Col span={8}>
                    <Card title="Total Users" bordered={false}>
                        1,234
                    </Card>
                </Col>
                <Col span={8}>
                    <Card title="Active Sessions" bordered={false}>
                        567
                    </Card>
                </Col>
                <Col span={8}>
                    <Card title="New Registrations" bordered={false}>
                        89
                    </Card>
                </Col>
            </Row>
        </DashboardLayout>
    );
};

export default withAuth(DashboardPage);
