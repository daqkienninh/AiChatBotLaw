import React, { useState } from "react";
import { Box, Container, Grid, ListItem, Typography } from "@mui/material";
import "./AdminDashboard.css";
import SidebarAdmin from "../../components/admin-components/SideBarAdmin";
import HeaderAdmin from "../../components/admin-components/HeaderAdmin";
import TeamSizeCard from "../../components/admin-components/TeamSizeCard";
import RevenueCard from "../../components/admin-components/RevenueCard";
import FixedIssuesCard from "../../components/admin-components/FixedIssuesCard";
import CompletedTasksCard from "../../components/admin-components/CompletedTasksCard";

const AdminDashboard = () => {
  const [drawerOpen, setDrawerOpen] = useState(false);

  const handleDrawerToggle = () => {
    setDrawerOpen(!drawerOpen);
  };

  return (
    <Box
      sx={{ display: "flex", backgroundColor: "#78bcc4", minHeight: "100vh" }}
    >
      <SidebarAdmin />
      <Box component="main" sx={{ flexGrow: 1, p: 2 }}>
        <HeaderAdmin handleDrawerToggle={handleDrawerToggle} />
        <div className="cards">
          <div className="left-card">
            <div className="top-item">
              <FixedIssuesCard />
              <FixedIssuesCard />
              <FixedIssuesCard />
            </div>
            <div className="bottom-item">
              <TeamSizeCard />
              <TeamSizeCard />
            </div>
          </div>
          <div className="right-card">
            <CompletedTasksCard style={{ height: "100vh" }} />
          </div>
        </div>
      </Box>
    </Box>
  );
};

export default AdminDashboard;
