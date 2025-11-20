import React, { useState } from "react";
import {
  Form,
  Input,
  Button,
  Card,
  Typography,
  message,
} from "antd";
import { Link, useNavigate } from "react-router-dom";
import { authService } from "../services/AuthService";

const { Title, Text } = Typography;

const Register = () => {
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null); // 🔹 error message state
  const navigate = useNavigate();

  const onFinish = async (values) => {
    if (values.password !== values.confirmPassword) {
      setErrorMessage("Passwords do not match!");
      return;
    }

    const payload = {
      username: values.username,
      password: values.password,
      email: values.email || null,
      role: "user",
    };

    setLoading(true);
    setErrorMessage(null); // reset previous errors

    try {
      await authService.register(payload);
      message.success("Registration successful!");
      navigate("/register-success");
    } catch (error) {
      console.error("Registration failed:", error);

      let messageText = "Registration failed. Please try again.";

      if (error.response) {
        if (error.response.status === 409) {
          messageText = "Username is already taken. Please choose another.";
        } else if (error.response.data?.message) {
          messageText = error.response.data.message;
        } else {
          messageText = "An unexpected server error occurred.";
        }
      } else if (error.request) {
        messageText = "No response from server. Please check your connection.";
      } else {
        messageText = "Unexpected error occurred.";
      }

      setErrorMessage(messageText);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: "60vh",
        background: "#ffe58f",
        borderRadius: "25px",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
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
        }}
      >
        <Title level={3} style={{ color: "#faad14", marginBottom: 8 }}>
          Create Your Account
        </Title>
        <Text type="secondary" style={{ display: "block", marginBottom: 24 }}>
          Join <b>EnerVision</b> today and start saving smarter.
        </Text>

        <Form name="register" layout="vertical" onFinish={onFinish}>
          <Form.Item
            label={<b>Username</b>}
            name="username"
            rules={[{ required: true, message: "Please enter your username!" }]}
          >
            <Input placeholder="Enter username" size="large" />
          </Form.Item>

          <Form.Item
            label={<b>Password</b>}
            name="password"
            rules={[{ required: true, message: "Please enter your password!" }]}
          >
            <Input.Password placeholder="Enter password" size="large" />
          </Form.Item>

          <Form.Item
            label={<b>Confirm Password</b>}
            name="confirmPassword"
            dependencies={["password"]}
            rules={[{ required: true, message: "Please confirm your password!" }]}
          >
            <Input.Password placeholder="Confirm password" size="large" />
          </Form.Item>

          <Form.Item label={<b>Email (optional)</b>} name="email">
            <Input placeholder="Enter email (optional)" size="large" />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Used only for personalized recommendations.
            </Text>
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              block
              size="large"
              loading={loading}
              style={{
                backgroundColor: "#faad14",
                borderColor: "#faad14",
                borderRadius: 8,
                fontWeight: 500,
              }}
            >
              {loading ? "Creating Account..." : "Register"}
            </Button>
          </Form.Item>

          {/* 🔹 Inline error display */}
          {errorMessage && (
            <Text type="danger" style={{ color: "red", display: "block", marginBottom: 16 }}>
              {errorMessage}
            </Text>
          )}

          <div style={{ marginTop: 8 }}>
            <Text type="secondary">
              Already have an account?{" "}
              <Link to="/login" style={{ color: "#fa8c16" }}>
                Login
              </Link>
            </Text>
          </div>
        </Form>
      </Card>
    </div>
  );
};

export default Register;
