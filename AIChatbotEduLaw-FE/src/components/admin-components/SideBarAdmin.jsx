import React from "react";
import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemText,
  Typography,
} from "@mui/material";

const SidebarAdmin = () => {
  const sidebarItems = [
    { text: "Dashboard", icon: "📊" },
    { text: "User Management", icon: "👤" },
    { text: "Legal Management", icon: "📋" },
    { text: "Clause Management", icon: "✍️" },
    { text: "Q&A Management", icon: "⭐" },
    { text: "Maps", icon: "🗺️" },
    { text: "Notifications", icon: "🔔" },
  ];

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: 240,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: 240,
          boxSizing: "border-box",
          backgroundColor: "#f7f8f3",
        },
      }}
    >
      <Box
        component="a"
        href="/home"
        sx={{
          display: "flex",
          alignItems: "center",
          mr: 2,
          textDecoration: "none",
          p: 2,
        }}
      >
        <Box
          component="img"
          src="/src/assets/edulawai.jpg"
          alt="EduLawAI"
          sx={{ height: 40, width: 40, objectFit: "contain" }}
        />
        <Typography
          variant="h6"
          component="div"
          sx={{ ml: 1, display: { xs: "none", sm: "block" } }}
        >
          EduLawAI
        </Typography>
      </Box>
      <Box sx={{ overflow: "auto" }}>
        <List>
          {sidebarItems.map((item) => (
            <ListItem
              button
              key={item.text}
              sx={{ "&:hover": { backgroundColor: "#e0e0e0" } }}
            >
              <ListItemText primary={`${item.icon} ${item.text}`} />
            </ListItem>
          ))}
        </List>
      </Box>
    </Drawer>
  );
};

export default SidebarAdmin;
