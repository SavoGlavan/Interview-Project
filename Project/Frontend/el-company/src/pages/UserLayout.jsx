import React, { useState } from "react";
import { Layout, Menu } from "antd";
import {
  HomeOutlined,
  UserOutlined,
} from "@ant-design/icons";
import PlanSelection from "../components/PlanSelection";
import UserDetails from "../components/UserDetails";


const { Header, Content } = Layout;

const UserLayout = () => {
  const [selectedMenu, setSelectedMenu] = useState("plan");

  const renderContent = () => {
    switch (selectedMenu) {
      case "plan":
        return <PlanSelection />;
      case "details":
        return <UserDetails/>;
      default:
        return <PlanSelection />;
    }
  };

  return (
    <Layout style={{ minHeight: "100vh", background:"#fffbe6"}}>
      <Header
        style={{
          background: "#ffffffff",
          boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          padding: 0,
        }}
      >
        <Menu
          mode="horizontal"
          selectedKeys={[selectedMenu]}
          onClick={(e) => setSelectedMenu(e.key)}
          style={{
            lineHeight: "64px",
            fontSize: "16px",
            fontWeight: 500,
            justifyContent: "center",
          }}
        >
          <Menu.Item key="plan" icon={<HomeOutlined />}>
            Plan Selection
          </Menu.Item>
          <Menu.Item key="details" icon={<UserOutlined />}>
            Profile
          </Menu.Item>
        </Menu>
      </Header>

      <Content style={{ padding: "24px" }}>
        {renderContent()}
      </Content>
    </Layout>
  );
};

export default UserLayout;
