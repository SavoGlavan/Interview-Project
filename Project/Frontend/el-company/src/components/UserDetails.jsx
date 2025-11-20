import React, { useEffect, useState } from "react";
import { Card, Descriptions, Button, Spin, message } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { userService } from "../services/UserService";
import { authService } from "../services/AuthService";

const UserDetails = () => {
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);

  const user = authService.getUserFromToken();

  useEffect(() => {
    const fetchUser = async () => {
      if (!user) {
        message.error("User not logged in.");
        return;
      }

      try {
        const data = await userService.getById(user.id);
        setUserData(data); // depending on your backend response
      } catch (err) {
        console.error(err);
        message.error("Failed to load user details.");
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, [user]);

  const handleLogout = () => {
    localStorage.removeItem("token");
    message.success("You have been logged out.");
    window.location.href = "/login";
  };

  if (loading) {
    return (
      <div style={{ textAlign: "center", padding: 50 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!userData) {
    return <p style={{ textAlign: "center" }}>No user data found.</p>;
  }

  return (
    <div style={{ display: "flex", justifyContent: "center", marginTop: 40 }}>
      <Card
        title="User Details"
        style={{
          width: 500,
          borderRadius: 16,
          boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
        }}
      >
        <Descriptions column={1} bordered size="middle">
          <Descriptions.Item label="Username">
            {userData.username}
          </Descriptions.Item>
          <Descriptions.Item label="Email">
            {userData.email || ""}
          </Descriptions.Item>
          <Descriptions.Item label="Plan">
            {userData.details.planName || "You have not selected to a plan!"}
          </Descriptions.Item>
          <Descriptions.Item label="Tax Group">
            {userData.details.taxGroupName || "You have not selected a tax group!"}
          </Descriptions.Item>
          <Descriptions.Item label="Consumption (kWh)">
            {userData.consumption || "You have not entered your consumption!"}
          </Descriptions.Item>
        </Descriptions>

        <Button
          type="primary"
          danger
          icon={<LogoutOutlined />}
          onClick={handleLogout}
          block
          style={{ marginTop: 24, width:70}}
        >
          Logout
        </Button>
      </Card>
    </div>
  );
};

export default UserDetails;
