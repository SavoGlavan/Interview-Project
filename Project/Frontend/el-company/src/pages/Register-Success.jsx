import React, { useEffect } from "react";
import { Card, Typography, Button } from "antd";
import { useNavigate } from "react-router-dom";
import { CheckCircleOutlined } from "@ant-design/icons";

const { Title, Text } = Typography;

const RegisterSuccess = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const timer = setTimeout(() => {
      navigate("/login");
    }, 5000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div
      style={{
        minHeight: "60vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        background: "#fffbe6",
        borderRadius: "25px",
        padding: "20px",
      }}
    >
      <Card
        style={{
          width: 420,
          textAlign: "center",
          borderRadius: 16,
          boxShadow: "0 8px 24px rgba(0,0,0,0.1)",
          background: "#fff",
          padding: "24px",
        }}
      >
        <CheckCircleOutlined
          style={{ fontSize: 64, color: "#52c41a", marginBottom: 16 }}
        />
        <Title level={3} style={{ color: "#faad14", marginBottom: 8 }}>
          Registration Successful!
        </Title>
        <Text type="secondary" style={{ display: "block", marginBottom: 16 }}>
          Your account has been created successfully.
          <br />
          You can now log in using your credentials.
        </Text>

        <Button
          type="primary"
          size="large"
          onClick={() => navigate("/login")}
          style={{
            backgroundColor: "#faad14",
            borderColor: "#faad14",
            borderRadius: 8,
            fontWeight: 500,
            marginTop: 12,
          }}
        >
          Go to Login
        </Button>

        <div style={{ marginTop: 16 }}>
          <Text type="secondary">
            Redirecting to login automatically in 5 seconds...
          </Text>
        </div>
      </Card>
    </div>
  );
};

export default RegisterSuccess;
