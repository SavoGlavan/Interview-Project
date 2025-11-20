import React from "react";
import { Button, Typography, Row, Col, Card } from "antd";
import {
  ThunderboltOutlined,
  DollarOutlined,
  PieChartOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";

const { Title, Paragraph } = Typography;

const LandingPage = () => {
  const navigate = useNavigate();

  return (
    <div
      style={{
        textAlign: "center",
        padding: "80px 24px",
        background: "#fffbe6", // soft yellow background
        minHeight: "100vh",
      }}
    >
      {/* Hero Section */}
      <Title
        level={1}
        style={{
          fontSize: "3rem",
          marginBottom: 16,
          color: "#1f1f1f",
        }}
      >
        Welcome to <span style={{ color: "#faad14" }}>EnerVision</span>
      </Title>
      <Paragraph
        style={{
          fontSize: "1.2rem",
          maxWidth: 600,
          margin: "0 auto 32px",
          color: "#555",
        }}
      >
        Compare, customize, and choose the best energy plan for your home or
        business. Smart savings, simple insights — all in one place.
      </Paragraph>
      <Button
        type="primary"
        size="large"
        style={{
          borderRadius: 8,
          background: "#faad14",
          borderColor: "#faad14",
          color: "#fff",
        }}
        onClick={() => navigate("/login")}
      >
        Get Started
      </Button>

      {/* Features Section */}
      <div style={{ marginTop: 80 }}>
        <Title level={2} style={{ color: "#1f1f1f" }}>
          Take Control of Your Energy Costs
        </Title>
        <Row
          gutter={[24, 24]}
          justify="center"
          style={{ marginTop: 32, maxWidth: 1000, marginInline: "auto" }}
        >
          <Col xs={24} md={8}>
            <Card
              bordered={false}
              style={{
                borderRadius: 12,
                boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
                background: "#fff",
              }}
            >
              <DollarOutlined
                style={{
                  fontSize: 40,
                  color: "#faad14",
                  marginBottom: 16,
                }}
              />
              <Title level={4}>Find the Best Price</Title>
              <Paragraph>
                Instantly compare plans and pick the one that saves you the most.
              </Paragraph>
            </Card>
          </Col>

          <Col xs={24} md={8}>
            <Card
              bordered={false}
              style={{
                borderRadius: 12,
                boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
                background: "#fff",
              }}
            >
              <ThunderboltOutlined
                style={{
                  fontSize: 40,
                  color: "#fa8c16",
                  marginBottom: 16,
                }}
              />
              <Title level={4}>Customize Your Plan</Title>
              <Paragraph>
                Choose flexible options tailored to your consumption and needs.
              </Paragraph>
            </Card>
          </Col>

          <Col xs={24} md={8}>
            <Card
              bordered={false}
              style={{
                borderRadius: 12,
                boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
                background: "#fff",
              }}
            >
              <PieChartOutlined
                style={{
                  fontSize: 40,
                  color: "#fadb14",
                  marginBottom: 16,
                }}
              />
              <Title level={4}>Understand Your Costs</Title>
              <Paragraph>
                See detailed breakdowns of pricing, taxes, and thresholds — no
                surprises.
              </Paragraph>
            </Card>
          </Col>
        </Row>
      </div>

      {/* Footer */}
      <Paragraph style={{ marginTop: 80, color: "#8c8c8c" }}>
        © {new Date().getFullYear()} EnerVision. Empowering smarter energy
        choices.
      </Paragraph>
    </div>
  );
};

export default LandingPage;
