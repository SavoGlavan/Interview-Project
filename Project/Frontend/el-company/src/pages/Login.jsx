import React, { useState } from "react";
import { Form, Input, Button, message, Card, Typography } from "antd";
import { useNavigate, Link } from "react-router-dom";
import { authService } from "../services/AuthService";

const { Title, Text } = Typography;

const Login = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null); // 🔹 inline error state

  const onFinish = async (values) => {
    setLoading(true);
    setErrorMessage(null); // reset previous error
    try {
      const response = await authService.login(values.username, values.password);

      const token = response.token;
      localStorage.setItem("token", token);
      message.success("Login successful!");

      const payload = JSON.parse(atob(token.split(".")[1]));
      const role =
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

      setTimeout(() => {
        if (role === "admin") navigate("/admin");
        else navigate("/home");
      }, 500);
    } catch (error) {
      console.error("Login failed:", error);

      if (error.response?.status === 401) {
        setErrorMessage("Invalid username or password.");
      } else if (error.response?.data?.message) {
        setErrorMessage(error.response.data.message);
      } else if (error.request) {
        setErrorMessage("No response from server. Please check your connection.");
      } else {
        setErrorMessage("Unexpected error occurred during login.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: "60vh",
        background: "#ffe58f",
        borderRadius: "20px",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        padding: "20px",
      }}
    >
      <Card
        style={{
          width: 380,
          textAlign: "center",
          borderRadius: 16,
          boxShadow: "0 8px 24px rgba(0,0,0,0.1)",
          background: "#fff",
        }}
      >
        <Title level={3} style={{ color: "#faad14", marginBottom: 8 }}>
          Welcome Back
        </Title>
        <Text type="secondary" style={{ display: "block", marginBottom: 24 }}>
          Sign in to continue to <b>EnerVision</b>
        </Text>

        <Form name="login" onFinish={onFinish} layout="vertical">
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
              {loading ? "Logging in..." : "Login"}
            </Button>
          </Form.Item>

          {/* 🔹 Inline error message */}
          {errorMessage && (
            <Text
              type="danger"
              style={{
                color: "red",
                display: "block",
                marginBottom: 16,
                textAlign: "center",
              }}
            >
              {errorMessage}
            </Text>
          )}

          <div style={{ marginTop: 8 }}>
            <Text type="secondary">
              Don’t have an account?{" "}
              <Link to="/register" style={{ color: "#fa8c16" }}>
                Register
              </Link>
            </Text>
          </div>
        </Form>
      </Card>
    </div>
  );
};

export default Login;
