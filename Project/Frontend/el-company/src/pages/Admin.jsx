import React, { useState } from "react";
import { Layout, Menu, message } from "antd";
import {
  AppstoreOutlined,
  DollarOutlined,
  BarChartOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import PlansDashboard from "../components/PlanDashboard";
import TaxGroupDashboard from "../components/TaxGroupDashboard";
// (you’ll later import Analytics.jsx here)
import Analytics from "../components/Analytics";

const { Sider, Content } = Layout;

const Admin = () => {
  const [selectedMenu, setSelectedMenu] = useState("plans");

  const handleMenuClick = (e) => {
    if (e.key === "logout") {
      localStorage.removeItem("token");
      message.success("Logged out successfully!");
      window.location.href = "/login";
      return;
    }

    setSelectedMenu(e.key);
  };

  const renderContent = () => {
    switch (selectedMenu) {
      case "plans":
        return <PlansDashboard />;
      case "tax":
        return <TaxGroupDashboard />;
      case "analytics":
        return <Analytics />;
      default:
        return null;
    }
  };

  return (
    <Layout style={{ minHeight: "50vh" }}>
      <Sider theme="light">
        <div
          style={{
            padding: 16,
            fontWeight: "bold",
            fontSize: 18,
            textAlign: "center",
          }}
        >
          Admin Panel
        </div>

        <Menu
          mode="inline"
          selectedKeys={[selectedMenu]}
          onClick={handleMenuClick}
          items={[
            { key: "plans", icon: <AppstoreOutlined />, label: "Plans" },
            { key: "tax", icon: <DollarOutlined />, label: "Tax Groups" },
            {
              key: "analytics",
              icon: <BarChartOutlined />,
              label: "Analytics",
            },
            { key: "logout", icon: <LogoutOutlined />, label: "Logout" },
          ]}
        />
      </Sider>

      <Layout style={{ minWidth: "100vh" }}>
        <Content style={{ padding: 24, background: "#f0f2f5" }}>
          {renderContent()}
        </Content>
      </Layout>
    </Layout>
  );
};

export default Admin;
