import React from "react";
import { Layout, Menu } from "antd";
import { useNavigate } from "react-router-dom";

const { Header } = Layout;

const AppHeader = () => {
  const navigate = useNavigate();

  return (
    <Header style={{ background: "#fff", boxShadow: "0 1px 4px rgba(0,0,0,0.1)" }}>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          maxWidth: 1200,
          margin: "0 auto",
        }}
      >
        <div
          onClick={() => navigate("/")}
          style={{ fontSize: 20, fontWeight: "bold", color: "#1677ff", cursor: "pointer" }}
        >
          EnerVision
        </div>
        <Menu mode="horizontal" selectable={false}>
          <Menu.Item onClick={() => navigate("/login")}>Login</Menu.Item>
          <Menu.Item onClick={() => navigate("/register")}>Register</Menu.Item>
        </Menu>
      </div>
    </Header>
  );
};

export default AppHeader;
